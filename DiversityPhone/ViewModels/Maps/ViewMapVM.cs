using System;
using System.Reactive.Linq;
using ReactiveUI;
using System.Windows;
using System.IO;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using Funq;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using System.Windows.Media.Imaging;

namespace DiversityPhone.ViewModels
{
    public class ViewMapVM : PageVMBase, ISavePageVM
    {
        private const double SCALEMIN = 0.2;
        private const double SCALEMAX = 3;

        private IMapStorageService MapStorage;
        private ILocationService Location;

        public ReactiveCommand SelectMap { get; private set; }
        public IReactiveCommand ToggleEditable { get; private set; }
        public ReactiveCommand SetLocation { get; private set; }
        public IReactiveCommand Save { get; private set; }

        public IElementVM<Map> CurrentMap { get { return _CurrentMap.Value; } }
        private ObservableAsPropertyHelper<IElementVM<Map>> _CurrentMap;

        public ILocalizable Current { get { return _Current.Value; } }
        private ObservableAsPropertyHelper<ILocalizable> _Current;

        public bool IsEditable { get { return _IsEditable.Value; } }
        private ObservableAsPropertyHelper<bool> _IsEditable;
        

        private double _Scale = 1.0;
        public double Scale
        {
            get { return _Scale; }
            set
            {
                if (value > SCALEMAX)
                    value = SCALEMAX;
                else if (value < SCALEMIN)
                    value = SCALEMIN;

                this.RaiseAndSetIfChanged(x => x.Scale, ref _Scale, value);
            }
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

        private Point? _CurrentLocalization = null;
        public Point? CurrentLocalization
        {
            get
            {
                return _CurrentLocalization;
            }
            private set
            {
                this.RaiseAndSetIfChanged(x => x.CurrentLocalization, ref _CurrentLocalization, value);
            }
        }

        public ViewMapVM(Container ioc)
        {
            MapStorage = ioc.Resolve<IMapStorageService>();
            Location = ioc.Resolve<ILocationService>();

            SelectMap = new ReactiveCommand();
            SelectMap
                .Select(_ => Page.MapManagement)
                .ToMessage();

            this.FirstActivation()
                .Select(_ => Page.MapManagement)
                .ToMessage();


            _CurrentMap = this.ObservableToProperty(Messenger.Listen<IElementVM<Map>>(MessageContracts.VIEW), x => x.CurrentMap);
            _CurrentMap
                .SelectMany(vm => Observable.Start(() => MapStorage.loadMap(vm.Model)).TakeUntil(_CurrentMap))                
                .ObserveOnDispatcher()
                .Select(stream => 
                    {
                        var img = new BitmapImage();
                        img.SetSource(stream);
                        return img;
                    })
                .BindTo(this, x => x.MapImage);

            _Current = this.ObservableToProperty(Messenger.Listen<ILocalizable>(MessageContracts.VIEW), x => x.Current);
            Observable.CombineLatest(
                _Current,
                _CurrentMap,
                (loc, map) => 
                    {
                        if(map == null)
                            return null;
                        if(loc != null && loc.Latitude.HasValue && loc.Longitude.HasValue)
                            return map.Model.PercentilePositionOnMap(loc.Latitude.Value, loc.Longitude.Value);
                        else
                            return null;
                    })                
                .Subscribe(c => CurrentLocalization = c);

            var current_is_localizable = _Current.Select(c => c != null);

            ToggleEditable = new ReactiveCommand(current_is_localizable);

            _IsEditable = this.ObservableToProperty(
                                _Current.Select(_ => false)
                                .Merge(ToggleEditable.Select(_ => true)),
                                x => x.IsEditable);

            SetLocation = new ReactiveCommand(_IsEditable);
            SetLocation
                .Select(loc => loc as Point?)
                .Where(loc => loc != null)
                .Subscribe(loc => CurrentLocalization = loc);

            var valid_localization = this.ObservableForProperty(x => x.CurrentLocalization).Value()
                .Select(loc => loc.HasValue);

            Save = new ReactiveCommand(_IsEditable.BooleanAnd(valid_localization));
            Save
                .Select(_ => Current)
                .Do(c => c.SetCoordinates(CurrentMap.Model.GPSFromPercentilePosition(CurrentLocalization.Value)))
                .ToMessage(MessageContracts.SAVE);

            ActivationObservable
                .Where(a => a)
                .Where(_ => CurrentMap != null)  
                .SelectMany(_ => Location.Location().TakeUntil(ActivationObservable.Where(a => !a)))
                .Select(c => CurrentMap.Model.PercentilePositionOnMap(c.Latitude.Value, c.Longitude.Value))
                .Subscribe(c => CurrentLocation = c);

        }
    }
}
