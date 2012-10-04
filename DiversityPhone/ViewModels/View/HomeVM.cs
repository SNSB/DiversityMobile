using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DiversityPhone.Messages;
using DiversityPhone.Model;
using DiversityPhone.Services;
using ReactiveUI;
using ReactiveUI.Xaml;
using Funq;
using System.Reactive.Disposables;
using System.Reactive;

namespace DiversityPhone.ViewModels
{


    public class HomeVM : PageViewModel
    {
        private ReactiveAsyncCommand getSeries = new ReactiveAsyncCommand();
                  
        private IFieldDataService Storage;
        private ILocationService Location;

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

        public HomeVM(Container ioc)           
        {
            Storage = ioc.Resolve<IFieldDataService>();
            Location = ioc.Resolve<ILocationService>();


            //EventSeries
            SeriesList = 
            getSeries.RegisterAsyncFunction(_ =>
                    Enumerable.Repeat(EventSeries.NoEventSeries,1)
                    .Concat(
                        Storage
                        .getAllEventSeries()
                        )                    
                    .Select(es => new EventSeriesVM(es))
                )
                .SelectMany(vm => vm)               
                .CreateCollection();            

            SeriesList
                    .ListenToChanges<EventSeries, EventSeriesVM>();    

            (SelectSeries = new ReactiveCommand<IElementVM<EventSeries>>())            
                .ToMessage(MessageContracts.VIEW);

            (EditSeries = new ReactiveCommand<IElementVM<EventSeries>>(vm => vm.Model != EventSeries.NoEventSeries))
                .ToMessage(MessageContracts.EDIT);

            

            var openSeries = SeriesList.Changed.Select(_ => Unit.Default)
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