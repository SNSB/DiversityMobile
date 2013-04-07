using System;
using System.Linq;
using System.Reactive.Linq;
using DiversityPhone.Model;
using DiversityPhone.Services;
using ReactiveUI;
using ReactiveUI.Xaml;

using System.Reactive;
using DiversityPhone.Interface;

namespace DiversityPhone.ViewModels
{


    public class HomeVM : PageVMBase
    {
        private readonly IFieldDataService Storage;
        private readonly ILocationService Location;

        private ReactiveAsyncCommand getSeries = new ReactiveAsyncCommand();
                  
        

        #region Commands
        public ReactiveCommand Settings { get; private set; }
        public ReactiveCommand Add { get; private set; }       
        public ReactiveCommand Maps { get; private set; }

        public ReactiveCommand<IElementVM<EventSeries>> SelectSeries { get; private set; }
        public ReactiveCommand<IElementVM<EventSeries>> EditSeries { get; private set; }
        #endregion        

        #region Properties              
        public ReactiveCollection<EventSeriesVM> SeriesList
        {
            get;
            private set;
        }       
        #endregion

        public HomeVM(
            IFieldDataService Storage,
            ILocationService Location
            )           
        {
            this.Storage = Storage;
            this.Location = Location;


            //EventSeries
            SeriesList = new ReactiveCollection<EventSeriesVM>();

            getSeries.RegisterAsyncFunction(_ =>
                    Enumerable.Repeat(EventSeries.NoEventSeries,1)
                    .Concat(
                        Storage
                        .getAllEventSeries()
                        )                    
                    .Select(es => new EventSeriesVM(es))
                )
                .SelectMany(vm => vm)               
                .Subscribe(SeriesList.Add);            

            SeriesList
                    .ListenToChanges<EventSeries, EventSeriesVM>();    

            (SelectSeries = new ReactiveCommand<IElementVM<EventSeries>>())            
                .ToMessage(MessageContracts.VIEW);

            (EditSeries = new ReactiveCommand<IElementVM<EventSeries>>(vm => vm.Model != EventSeries.NoEventSeries))
                .ToMessage(MessageContracts.EDIT);

            

            var openSeries = SeriesList.CollectionCountChanged.Select(_ => Unit.Default)
                .Merge(Messenger.Listen<IElementVM<EventSeries>>(MessageContracts.SAVE).Select(_ => Unit.Default))
                .Select(_ => SeriesList.Where(s => s.Model.SeriesEnd == null))
                .Select(list => list.FirstOrDefault());

            openSeries
                .SelectMany(series => (series != null) ?
                    Location.LocationByDistanceThreshold(20)
                    .Select(c =>
                        {
                            var gp = new GeoPointForSeries() { SeriesID = series.Model.SeriesID.Value };
                            gp.SetCoordinates(c);
                            return gp;
                        })
                    .TakeUntil(openSeries) : Observable.Empty<GeoPointForSeries>())
                .ObserveOnDispatcher()
                .ToMessage(MessageContracts.SAVE);
                

            var noOpenSeries = 
                openSeries
                .Select(openseries => openseries == null);

            Settings = new ReactiveCommand();
            Settings.Select(_ => Page.Settings)
                .ToMessage();

            Add = new ReactiveCommand(noOpenSeries);
            Add.Select(_ => new EventSeriesVM(new EventSeries()) as IElementVM<EventSeries>)
                .ToMessage(MessageContracts.EDIT);

            Maps = new ReactiveCommand();
            Maps.Select(_ => null as ILocalizable)
                .ToMessage(MessageContracts.VIEW);

            Observable.Merge(
                Messenger.Listen<EventMessage>(MessageContracts.CLEAN),
                Messenger.Listen<EventMessage>(MessageContracts.INIT),
                Messenger.Listen<EventMessage>(MessageContracts.REFRESH)
                )                
                .Do(_ => SeriesList.Clear())
                .Subscribe(_ => getSeries.Execute(null));
        }
    }
}