using DiversityPhone.ViewModels;
using Microsoft.Phone.Shell;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace DiversityPhone.View
{
    public class ViewAudioVideoPageAppbarUpdater 
    {
        IApplicationBar _appbar;
        IAudioVideoPageVM _vm;
        ApplicationBarIconButton  _edit, _delete,_play,_stop;

        public ViewAudioVideoPageAppbarUpdater(IApplicationBar appbar, IAudioVideoPageVM viewmodel)
        {
            _appbar = appbar;
            _vm = viewmodel;

            if (_vm == null)
                return;


            _play = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/AudioIcons/play.png", UriKind.Relative),
                Text = "play",
                IsEnabled = true,
            };
            _play.Click += (s, args) => _vm.Play.Execute(null);
            _play.Click += (s, args) => this.adjustPlaying(true);

            _stop = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/AudioIcons/stop.png", UriKind.Relative),
                Text = "stop",
                IsEnabled = false,
            };
            _stop.Click += (s, args) => _vm.Stop.Execute(null);
            _stop.Click += (s, args) => this.adjustPlaying(false);


            _vm.ObservableForProperty(x => x.IsEditable)
                .Select(change => change.Value)
                .Subscribe(iseditable => adjustApplicationBar(iseditable));

            _edit = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.edit.rest.png", UriKind.Relative),
                Text = "edit",
                IsEnabled = true,
            };
            _edit.Click += (s, args) => _vm.ToggleEditable.Execute(null);
            _delete = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.delete.rest.png", UriKind.Relative),
                Text = "delete",
                IsEnabled = true,
            };
            _delete.Click += (s, args) => _vm.Delete.Execute(null);

            adjustApplicationBar(_vm.IsEditable);

        }

        private void adjustApplicationBar(bool editable)
        {
            {
                _appbar.Buttons.Clear();
                if (editable == true)
                {
                    _appbar.Buttons.Add(_delete);
                }
                else
                {
                    _appbar.Buttons.Add(_play);
                    _appbar.Buttons.Add(_stop);
                    _appbar.Buttons.Add(_edit);
                }
            }
        }

        public void adjustPlaying(bool playing)
        {
            if (playing == true)
            {
                _play.IsEnabled = false;
                _stop.IsEnabled = true;
                _edit.IsEnabled = false;
            }
            else
            {
                _play.IsEnabled = true;
                _stop.IsEnabled = false;
                _edit.IsEnabled = true;
            }
        }

    }
}
