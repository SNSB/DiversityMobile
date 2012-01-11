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

namespace DiversityPhone.ViewModels
{
    public class ViewLMVM : PageViewModel
    {
        private IList<IDisposable> _subscriptions;    

        #region Commands
        public ReactiveCommand AddMaps { get; private set; }
        #endregion

        public ViewLMVM(IMessageBus messenger)
            : base(messenger)
        {
            
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


    }
}
