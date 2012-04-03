using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Services;
using DiversityPhone.Messages;
using DiversityPhone.Model;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public class ViewLMVM : PageViewModel
    {
        private IList<IDisposable> _subscriptions;
        private IFieldDataService _storage;

        #region Properties 

        private ObservableAsPropertyHelper<IList<MapVM>> _SavedMaps;
        public IList<MapVM> SavedMaps
        {
            get { return _SavedMaps.Value; }
        }

        #endregion

        #region Commands
        public ReactiveCommand AddMaps { get; private set; }

        #endregion

        public ViewLMVM(IFieldDataService storage)  
        {
            _storage = storage;
            IList<Map> maps=_storage.getAllMaps();
            IList<MapVM> mapModels = new List<MapVM>();
            foreach(Map map in maps)
                mapModels.Add(new MapVM(Messenger,map,Page.ViewMap));

            _SavedMaps = StateObservable
                .Select(_ => updatedMapList())
                .ToProperty(this, x => x.SavedMaps);

            _subscriptions = new List<IDisposable>()
            {
                (AddMaps = new ReactiveCommand())
                    .Subscribe(_ => addMaps()),
                          
            };

        }


        public void saveMap(Map map)
        {
            _storage.addOrUpdateMap(map);

            _SavedMaps = StateObservable
                .Select(_ => updatedMapList())
                .ToProperty(this, x => x.SavedMaps);
            
        }

        private void addMaps()       
        {
            Messenger.SendMessage<NavigationMessage>(new NavigationMessage(Page.DownLoadMaps, null));
        }

        private IList<MapVM> updatedMapList()
        {
            return new VirtualizingReadonlyViewModelList<Map, MapVM>(
                _storage.getAllMaps(),
                (model) => new MapVM(Messenger, model, Page.ViewMap)
                );
        } 
       
    }
}
