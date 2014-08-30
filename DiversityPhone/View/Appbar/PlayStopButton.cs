using DiversityPhone.ViewModels;
using Microsoft.Phone.Shell;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace DiversityPhone.View.Appbar
{
    public class PlayStopButton : SwitchingCommandButtonAdapter
    {
        private readonly ButtonState PLAY, STOP;
        private IDisposable _subscription;

        public PlayStopButton(IApplicationBar appbar, IAudioVideoPageVM vm)
            : base(appbar)
        {
            PLAY = new ButtonState()
            {
                URI = new Uri("/Images/AudioIcons/play.png", UriKind.Relative),
                Text = DiversityResources.Button_Play,
                Command = vm.Play
            };

            STOP = new ButtonState()
            {
                URI = new Uri("/Images/AudioIcons/stop.png", UriKind.Relative),
                Text = DiversityResources.Button_Stop,
                Command = vm.Stop
            };

            _subscription =
            vm.ObservableForProperty(x => x.State)
            .Value()
            .StartWith(vm.State)
            .Select(state => (state == PlayStates.Idle) ? PLAY : STOP)
            .Subscribe(s => this.CurrentState = s);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this._subscription.Dispose();
            base.Dispose(disposing);
        }
    }
}