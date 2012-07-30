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
        private IList<IDisposable> _subscriptions;

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

           

            (SelectSeries = new ReactiveCommand<IElementVM<EventSeries>>())            
                .ToMessage(MessageContracts.VIEW);

            (EditSeries = new ReactiveCommand<IElementVM<EventSeries>>(vm => vm.Model != EventSeries.NoEventSeries))
                .ToMessage(MessageContracts.EDIT);
                

            var noOpenSeries = SeriesList
                .Changed
                .Select(_ => SeriesList)
                .Select(list => list.Any(s => s.Model.SeriesEnd == null))
                .Select(openseries => !openseries);

            _subscriptions = new List<IDisposable>()
            {
                (Settings = new ReactiveCommand())
                    .Subscribe(_ => Messenger.SendMessage<Page>(Page.Settings)),                
                (Add = new ReactiveCommand(noOpenSeries))
                    .Subscribe(_ => addSeries()),               
                (Maps=new ReactiveCommand())
                    .Subscribe(_ =>loadMapPage()),  
                SeriesList
                    .ListenToChanges<EventSeries, EventSeriesVM>()
            };

            getSeries.Execute(null);
        }         

        private void addSeries()
        {
            Messenger.SendMessage<NavigationMessage>(new NavigationMessage(Page.EditES,null));
        }

        private void loadMapPage()
        {
            Messenger.SendMessage<NavigationMessage>(new NavigationMessage(Page.LoadedMaps, null));
        }


       
    }
}