
namespace DiversityPhone.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using Svc = DiversityPhone.DiversityService;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using DiversityPhone.Services;
    using DiversityPhone.Model;
    using DiversityPhone.Messages;
    using System.IO.IsolatedStorage;
    using System.IO;
    using System.Collections.ObjectModel;
    using GlobalUtility;
    using System.Windows;


    public class HomeVM : PageViewModel
    {
        private IList<IDisposable> _subscriptions;
        
        #region Services        
        private IFieldDataService _storage;
        private IDiversityServiceClient _repository;
        private ISettingsService _settings;       
        #endregion

        #region Commands
        public ReactiveCommand Settings { get; private set; }
        public ReactiveCommand Add { get; private set; }       
        public ReactiveCommand Maps { get; private set; }              
        #endregion

        private ReactiveAsyncCommand getSeries = new ReactiveAsyncCommand();

        #region Properties              
        public ReactiveCollection<EventSeriesVM> SeriesList
        {
            get;
            private set;
        }

        public EventSeriesVM NoEventSeries { get; private set; }
        #endregion

        public HomeVM(IMessageBus messenger, IFieldDataService storage, IDiversityServiceClient repo, ISettingsService settings)
            : base(messenger)
        {            
            _storage = storage;            
            _settings = settings;

            NoEventSeries = new EventSeriesVM(EventSeries.NoEventSeries);
            NoEventSeries
                .SelectObservable                
                .ToNavigation(Page.ViewES);

            SeriesList = 
            getSeries.RegisterAsyncFunction(_ =>
                storage.getAllEventSeries()
                .Select(es => new EventSeriesVM(es))
                )
                .SelectMany(vm => vm)
                .CreateCollection();

                 

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
                    .ListenToChanges()
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