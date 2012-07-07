
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
        private ISettingsService _settings;       
        #endregion

        #region Commands
        public ReactiveCommand Settings { get; private set; }
        public ReactiveCommand Add { get; private set; }       
        public ReactiveCommand Maps { get; private set; }         
        #endregion

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
                .Select(_ => (string)null)
                .ToNavigation(Page.ViewES);     

            
            var series = StateObservable
                .Select(_ => storage.getAllEventSeries())
                .Publish();
            series.Connect();

            SeriesList =
                series
                .Do(_ => SeriesList.Clear())
                .SelectMany(list => list.Select(s => new EventSeriesVM(s)))
                .Do(vm => vm.SelectObservable
                    .Select(sender => sender.Model.SeriesID.ToString())
                    .ToNavigation(Page.ViewES)
                    )
                .CreateCollection();      

            var noOpenSeries = series
                .Select(list => list.Any(s => s.SeriesEnd == null))
                .Select(openseries => !openseries);

            _subscriptions = new List<IDisposable>()
            {
                (Settings = new ReactiveCommand())
                    .Subscribe(_ => Messenger.SendMessage<Page>(Page.Settings)),    
                
                (Add = new ReactiveCommand(noOpenSeries))
                    .Subscribe(_ => addSeries()),                
                (Maps=new ReactiveCommand())
                    .Subscribe(_ =>loadMapPage()),                
            };

            
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