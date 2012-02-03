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
        private  Dictionary<String, String> _savedMaps = null;
        public Dictionary<String,String> SavedMaps
        {
            get { return _savedMaps; }
            set { this.RaiseAndSetIfChanged(x => x.SavedMaps, ref _savedMaps, value); }
        }

        #region Commands
        public ReactiveCommand AddMaps { get; private set; }
        public ReactiveCommand LoadMaps { get; private set; }
        #endregion

        public ViewLMVM(IMessageBus messenger)
            : base(messenger)
        {
            
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
            Dictionary<String,String> maps = new Dictionary<String, String>();
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IList<String> mapimages = isoStore.GetFileNames("Maps\\MapImages\\*");
                foreach (String mapimage in mapimages)
                {
                    String name = mapimage.Substring(0, mapimage.LastIndexOf("."));
                    String xmlname = name + ".xml";
                    if (isoStore.FileExists("Maps\\XML\\" + xmlname))
                    {
                        MapParameter mp = MapParameter.loadMapParameterFromFile("Maps\\XML\\" + xmlname);
                        maps.Add(name, name + " - " + mp.Description);
                    }
                    else maps.Add(name, name);
                }
                SavedMaps = maps;  
            }
        }

        private void addMaps()
        {
            Messenger.SendMessage<NavigationMessage>(new NavigationMessage(Page.DownLoadMaps, null));
        }


    }
}
