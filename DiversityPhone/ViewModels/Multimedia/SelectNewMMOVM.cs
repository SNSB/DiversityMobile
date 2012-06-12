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
using System.Reactive.Subjects;

namespace DiversityPhone.ViewModels
{
    public class SelectNewMMOVM : PageViewModel
    {
        private IList<IDisposable> _subscriptions;

        #region Commands
        public ReactiveCommand SelectImage { get; private set; }
        public ReactiveCommand SelectAudio { get; private set; }
        public ReactiveCommand SelectVideo { get; private set; }

        #endregion

        ISubject<Page> DestinationSelected = new Subject<Page>();

        public SelectNewMMOVM()
        {

            var lastState = StateObservable
                .Replay(1);
            Messenger.RegisterMessageSource(
            DestinationSelected
                .Select(dest =>
                    {
                        var latestState = lastState.First();
                        return new NavigationMessage(dest, null, latestState.ReferrerType, latestState.Referrer);
                    }));



            _subscriptions = new List<IDisposable>()
            {
             (SelectImage = new ReactiveCommand())
                    .Select(_ => Page.NewImage)
                    .Subscribe(DestinationSelected.OnNext),
            (SelectAudio = new ReactiveCommand())
                .Select(_ => Page.NewAudio)
                    .Subscribe(DestinationSelected.OnNext),
            (SelectVideo = new ReactiveCommand())
                .Select(_ => Page.NewVideo)
                    .Subscribe(DestinationSelected.OnNext),    
            };
        }


        

    }
}
