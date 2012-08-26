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

namespace DiversityPhone.ViewModels
{


    public class HomeVM : PageViewModel
    {
        private ReactiveAsyncCommand getSeries = new ReactiveAsyncCommand();
        
        #region Services        
        private IFieldDataService Storage;
        #endregion

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
                

            var noOpenSeries = SeriesList
                .Changed
                .Select(_ => SeriesList)
                .Select(list => list.Any(s => s.Model.SeriesEnd == null))
                .Select(openseries => !openseries);

            Settings = new ReactiveCommand();
            Settings.Select(_ => Page.Settings)
                .ToMessage();
            Add = new ReactiveCommand(noOpenSeries);
            Add.Select(_ => new EventSeriesVM(new EventSeries()) as IElementVM<EventSeries>)
                .ToMessage(MessageContracts.EDIT);
            Maps=new ReactiveCommand();


            Maps.Select(_ => null as ILocalizable)
                .ToMessage(MessageContracts.VIEW);

            Observable.Merge(
                Messenger.Listen<EventMessage>(MessageContracts.CLEAN),
                Messenger.Listen<EventMessage>(MessageContracts.INIT)
                )                
                .Do(_ => SeriesList.Clear())
                .Subscribe(_ => getSeries.Execute(null));
        }
    }
}