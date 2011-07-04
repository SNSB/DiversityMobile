using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.Xaml;
using System.Reactive.Linq;
using SterlingToLINQ.DiversityService;



namespace SterlingToLINQ
{
    public class MainViewModel : ReactiveObject
    {
        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ReactiveCollection<ItemViewModel> Items { get; private set; }
        private ObservableCollection<CollectionEvent> _items;

        private ReactiveAsyncCommand _FillItems;

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        private string _InsertTitle = "Test";
        
        public string InsertTitle 
        { 
            get
            {
                return _InsertTitle;
            }
            set
            {
                this.RaiseAndSetIfChanged(vm => vm.InsertTitle,value);
            }
        }      

        public ICommand Insert { get; protected set; }

        public ICommand LoadServiceData { get; protected set; }


        public MainViewModel()
        {
            this._items = new ObservableCollection<CollectionEvent>();
            //Items = _items.CreateDerivedCollection<CollectionEvent, ItemViewModel>(model => new ItemViewModel(model));

            _FillItems = new ReactiveAsyncCommand();
            

            var titleValid = this.ObservableForProperty(vm => vm.InsertTitle)
                .Select(title => !string.IsNullOrEmpty(title.Value));                

            var insert = new ReactiveCommand(titleValid);
            insert.Subscribe(_ => insertLines(InsertTitle));
            Insert = insert;

            LoadServiceData = ReactiveCommand.Create(null, _ => App.Repository.GetEventsAsync(0, 100));


            var serviceResults = Observable.FromEventPattern<GetEventsCompletedEventArgs>(App.Repository,"GetEventsCompleted");
            serviceResults.Subscribe(eventPattern => storeEvents(eventPattern.EventArgs.Result));
        }

        private void storeEvents(IEnumerable<CollectionEvent> events)
        {
            if (events != null)
            {
                App.Database.Truncate(typeof(CollectionEvent));
                foreach (var ev in events)
                {
                    App.Database.Save(ev);
                }
            }
        }

        private void insertLines(string title)
        {
            var ev = new CollectionEvent(){LocalityDescription = title, CollectionDate = DateTime.Now.Date};
            App.Repository.AddEventAsync(ev);
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {   
            foreach (var item in App.Database.Query<CollectionEvent, Guid>())
            {
                _items.Add(item.LazyValue.Value);
            }            
        }

        




       
    }
}