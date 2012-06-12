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

namespace DiversityPhone.ViewModels.Multimedia
{
    public class SelectNewMMOVM : PageViewModel
    {
        private IList<IDisposable> _subscriptions;

        #region Commands
        public ReactiveCommand SelectImage { get; private set; }
        public ReactiveCommand SelectAudio { get; private set; }
        public ReactiveCommand SelectVideo { get; private set; }

        #endregion

        public SelectNewMMOVM()
        {
            //PageState p = this.ObservableForProperty(StateObservable.Select(;
            _subscriptions = new List<IDisposable>()
            {
                (SelectImage = new ReactiveCommand())
                    .Subscribe(_ => (StateObservable.Select(page =>selectImage(page)))),
                //(SelectAudio = new ReactiveCommand())
                //    .Subscribe(_ => selectAudio(p)),
                //(SelectVideo = new ReactiveCommand())
                //    .Subscribe(_ => selectVideo(p)),    
            };
        }


        private void selectImage(PageState s)
        {
            Messenger.SendMessage(new NavigationMessage(Page.NewImage, null,s.ReferrerType, s.Referrer));
        }

        private void selectAudio(PageState s)
        {
            Messenger.SendMessage(new NavigationMessage(Page.NewAudio, null, s.ReferrerType, s.Referrer));
        }

        private void selectVideo(PageState s)
        {
            Messenger.SendMessage(new NavigationMessage(Page.NewVideo, null, s.ReferrerType, s.Referrer));
        }

    }
}
