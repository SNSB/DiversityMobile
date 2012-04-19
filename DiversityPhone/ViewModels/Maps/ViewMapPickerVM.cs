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
using System.Collections.ObjectModel;

namespace DiversityPhone.ViewModels
{
    public class ViewMapPickerVM : PageViewModel
    {
        private IList<IDisposable> _subscriptions;
        private IMapStorageService _maps;

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

        public ViewMapPickerVM(IMapStorageService maps)  
        {
            _maps = maps;

            _SavedMaps = StateObservable
                .Select(p => updatedMapList(p))
                .ToProperty(this, x => x.SavedMaps);

            _subscriptions = new List<IDisposable>()
            {
                (AddMaps = new ReactiveCommand())
                    .Subscribe(_ => addMaps()),                      
            };

        }

        private void addMaps()       
        {
            Messenger.SendMessage<NavigationMessage>(new NavigationMessage(Page.DownLoadMaps, null));
        }

        private IList<MapVM> updatedMapList(PageState s)
        {
            
            return new ObservableCollection<MapVM>(
                _maps.getAllMaps().Select(
                (model) => new MapVM(Messenger, model, Page.ViewMap, s.ReferrerType,s.Referrer)
                ));
        }

        private bool saveReferrer(PageState s)
        {

            return true;
        }
    }
}
