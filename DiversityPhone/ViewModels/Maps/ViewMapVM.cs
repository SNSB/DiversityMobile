using System;
using System.Reactive.Linq;
using ReactiveUI;
using System.Windows;
using DiversityPhone.Model;

using DiversityPhone.Services;
using ReactiveUI.Xaml;
using System.Windows.Media.Imaging;
using System.Reactive.Concurrency;
using DiversityPhone.Interface;
using System.Diagnostics.Contracts;

namespace DiversityPhone.ViewModels
{
    public class ViewMapVM : PageVMBase, ISavePageVM
    {
        readonly IMapStorageService MapStorage;
        readonly ILocationService Location;
        readonly IFieldDataService Storage;

        public ReactiveCommand SelectMap { get; private set; }
        public IReactiveCommand ToggleEditable { get; private set; }
        public ReactiveCommand SetLocation { get; private set; }
        public IReactiveCommand Save { get; private set; }

        public IElementVM<Map> CurrentMap { get { return _CurrentMap.Value; } }
        private ObservableAsPropertyHelper<IElementVM<Map>> _CurrentMap;

        public double ImageScale { get; set; }

        public Point ImageOffset { get; set; }
        
        public bool IsEditable { get { return _IsEditable.Value; } }
        private ObservableAsPropertyHelper<bool> _IsEditable;

        private string _MapUri;
        public string MapUri
        {
            get { return _MapUri; }
            set { this.RaiseAndSetIfChanged(x => x.MapUri, ref _MapUri, value); }
        }

        

        private BitmapImage _MapImage;
        public BitmapImage MapImage
        {
            get
            {
                return _MapImage;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.MapImage, ref _MapImage, value);
            }
        }


        private Point? _CurrentLocation = null;
        public Point? CurrentLocation
        {
            get
            {
                return _CurrentLocation;
            }
            private set
            {
                this.RaiseAndSetIfChanged(x => x.CurrentLocation, ref _CurrentLocation, value);
            }
        }

        private Point? _PrimaryLocalization = null;
        public Point? PrimaryLocalization
        {
            get
            {
                return _PrimaryLocalization;
            }
            private set
            {
                this.RaiseAndSetIfChanged(x => x.PrimaryLocalization, ref _PrimaryLocalization, value);
            }
        }

        private IObservable<IObservable<Point?>> _AdditionalLocalizations;
        public IObservable<IObservable<Point?>> AdditionalLocalizations
        {
            get
            {
                return _AdditionalLocalizations;
            }           
        }

        public ViewMapVM(
            IMapStorageService MapStorage,
            ILocationService Location,
            IFieldDataService Storage
            )
        {
            Contract.Requires(MapStorage != null);
            Contract.Requires(Location != null);
            Contract.Requires(Storage != null);
            this.MapStorage = MapStorage;
            this.Location = Location;
            this.Storage = Storage;

            ImageScale = 1.0;
            ImageOffset = new Point();

            SelectMap = new ReactiveCommand();
            SelectMap
                .Select(_ => Page.MapManagement)
                .ToMessage();

            this.FirstActivation()
                .Select(_ => Page.MapManagement)
                .ToMessage();


            _CurrentMap = this.ObservableToProperty(Messenger.Listen<IElementVM<Map>>(MessageContracts.VIEW), x => x.CurrentMap);
            _CurrentMap
                .Where(vm => vm!=null)
                .SelectMany(vm => Observable.Start(() => MapStorage.loadMap(vm.Model)).TakeUntil(_CurrentMap))                
                .ObserveOnDispatcher()
                .Select(stream => 
                    {
                        var img = new BitmapImage();
                        img.SetSource(stream);
                        stream.Close();
                        return img;
                    })
                .Subscribe(x => MapImage = x);

            var current_series = Messenger.Listen<ILocationOwner>(MessageContracts.VIEW);

            var current_localizable = Messenger.Listen<ILocalizable>(MessageContracts.VIEW);

            var current_series_if_not_localizable = current_series.Merge(current_localizable.Select(_ => null as ILocationOwner));

            var current_localizable_if_not_series = current_localizable.Merge(current_series.Select(_ => null as ILocalizable));

            var series_and_map = 
            current_series_if_not_localizable                
                .CombineLatest(_CurrentMap.Where(x => x != null), (es, map) =>
                    new { Map = map.Model, Series = es })
                .Publish();

            
            var add_locs =
            series_and_map
                .Select(pair =>
                    {
                        if (pair.Series != null)
                        {
                            var stream = Storage.getGeoPointsForSeries(pair.Series.EntityID).ToObservable(ThreadPoolScheduler.Instance) //Fetch geopoints asynchronously on Threadpool thread
                                    .Merge(Messenger.Listen<GeoPointForSeries>(MessageContracts.SAVE).Where(gp => gp.SeriesID == pair.Series.EntityID)) //Listen to new Geopoints that are added to the current tour
                                    .Select(gp => pair.Map.PercentilePositionOnMap(gp))
                                    .TakeUntil(series_and_map)
                                    .Replay();
                            stream.Connect();
                            return stream as IObservable<Point?>;
                        }
                        else
                            return Observable.Empty<Point?>();
                    }).Replay(1);

            _AdditionalLocalizations = add_locs;
            add_locs.Connect();

            series_and_map.Connect();

            Observable.CombineLatest(
                current_localizable_if_not_series,
                _CurrentMap,
                (loc, map) => 
                    {
                        if(map == null)
                            return null;
                        return map.Model.PercentilePositionOnMap(loc);                        
                    })                
                .Subscribe(c => PrimaryLocalization = c);

            

            ToggleEditable = new ReactiveCommand(current_localizable_if_not_series.Select(l => l != null));

            _IsEditable = this.ObservableToProperty(
                                current_localizable_if_not_series.Select(_ => false)
                                .Merge(ToggleEditable.Select(_ => true)),
                                x => x.IsEditable);

            SetLocation = new ReactiveCommand(_IsEditable);
            SetLocation
                .Select(loc => loc as Point?)
                .Where(loc => loc != null)
                .Subscribe(loc => PrimaryLocalization = loc);

            var valid_localization = this.ObservableForProperty(x => x.PrimaryLocalization).Value()
                .Select(loc => loc.HasValue);


            
            Save = new ReactiveCommand(_IsEditable.BooleanAnd(valid_localization));
            current_localizable_if_not_series
                .Where(loc => loc != null)
                .SelectMany(loc => 
                    Save
                    .Select(_ => loc)                
                    .TakeUntil(current_localizable_if_not_series)
                    )
                .Do(c => c.SetCoordinates(CurrentMap.Model.GPSFromPercentilePosition(PrimaryLocalization.Value)))
                .Do(_ => Messenger.SendMessage(Page.Previous))
                .ToMessage(MessageContracts.SAVE);

            ActivationObservable
                .Where(a => a)
                .Where(_ => CurrentMap != null)
                .SelectMany(_ => Location.Location().StartWith(null as Coordinate).TakeUntil(ActivationObservable.Where(a => !a)))                
                .Select(c => CurrentMap.Model.PercentilePositionOnMap(c))                
                .Subscribe(c => CurrentLocation = c);

        }
    }
}
