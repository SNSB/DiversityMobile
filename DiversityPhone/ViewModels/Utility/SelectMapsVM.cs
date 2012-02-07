using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Services;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{
    public class SelectMapsVM :PageViewModel
    {
        private IList<IDisposable> _subscriptions;
        private Dictionary<String, String> _savedMaps = null;
        public Dictionary<String, String> SavedMaps
        {
            get { return _savedMaps; }
            set { this.RaiseAndSetIfChanged(x => x.SavedMaps, ref _savedMaps, value); }
        }

        #region Commands
        public ReactiveCommand Select { get; private set; }
        #endregion

        public SelectMapsVM(IMessageBus messenger)
            : base(messenger)
        {

            _subscriptions = new List<IDisposable>()
            {
              
                
                (Select = new ReactiveCommand())
                    .Subscribe(_ => selectMap()),
               
            };
        }

        private void selectMap()
        {
            Messenger.SendMessage<NavigationMessage>(new NavigationMessage(DiversityPhone.Services.Page.ViewMap, null));
        }

    }
}
