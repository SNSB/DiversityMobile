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

namespace DiversityPhone.ViewModels
{
    public class ViewLMVM : PageViewModel
    {
        private IList<IDisposable> _subscriptions;


        #region Services
        private IFieldDataService _storage;
        #endregion

        #region Properties
        private IList<Map> _savedMaps;
        public IList<Map> SavedMaps
        {
            get { return _savedMaps; }
            set { this.RaiseAndSetIfChanged(x => x.SavedMaps, ref _savedMaps, value); }
        }

        #endregion

        #region Commands
        public ReactiveCommand AddMaps { get; private set; }
        public ReactiveCommand LoadMaps { get; private set; }
        #endregion

        public ViewLMVM(IMessageBus messenger, IFieldDataService storage)
            : base(messenger)
        {

            _storage = storage;
            _subscriptions = new List<IDisposable>()
            {
                (AddMaps = new ReactiveCommand())
                    .Subscribe(_ => addMaps()),
                      
                (LoadMaps = new ReactiveCommand())
                    .Subscribe(_ => loadMaps()),
               
            };

        }

        private void loadMaps()
        {
            //Get Mapd From DB
           
        }

        public void saveMap(Map map)
        {
            _storage.addOrUpdateMap(map);
        }

        private void addMaps()
        {
            Messenger.SendMessage<NavigationMessage>(new NavigationMessage(Page.DownLoadMaps, null));
        }


    }
}
