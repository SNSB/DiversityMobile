using System;
using System.Net;
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
using Microsoft.Phone.Shell;
using DiversityPhone.ViewModels;
using ReactiveUI;
using System.Reactive.Linq;
using DiversityPhone.Model;
using System.Windows.Media.Imaging;
namespace DiversityPhone.View.Appbar
{
    public class NewAudioAppBarUpdater
    {
        IApplicationBar _appbar;
        NewAudioVM _vm;
        ApplicationBarIconButton _record,_play,_stop, _save;

        public NewAudioAppBarUpdater(IApplicationBar appbar, NewAudioVM viewmodel)
        {
            _appbar = appbar;
            _vm = viewmodel;

            if (_vm == null)
                return;

            _record = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.feature.audio.rest.png",UriKind.Relative),
                Text = "record",
                IsEnabled = true,
            };
            _record.Click += (s,args) => _vm.Record.Execute(null);

             _play = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/AudioIcons/play.png", UriKind.Relative),
                Text = "play",
                IsEnabled = true,
            };
            _play.Click += (s,args) => _vm.Play.Execute(null);


            _stop = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/AudioIcons/stop.png", UriKind.Relative),
                Text = "stop",
                IsEnabled = true,
            };
            _stop.Click += (s, args) => _vm.Stop.Execute(null);

             _save = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.save.rest.png",UriKind.Relative),
                Text = "save",
                IsEnabled=true
            };
            _save.Click += (s,args) => _vm.Save.Execute(null);

            _appbar.Buttons.Add(_record);
            _appbar.Buttons.Add(_play);
            _appbar.Buttons.Add(_stop);
            _appbar.Buttons.Add(_save);

            _vm.Save.CanExecuteObservable
               .StartWith(_vm.Save.CanExecute(null))
               .Subscribe(canSave => _save.IsEnabled = canSave);

            _vm.ObservableForProperty(x => x.State)
             .Value()
             .StartWith(_vm.State)
             .Subscribe(state => adjustApplicationBar(state));

        }

        public void adjustApplicationBar(PlayStates state)
        {
            switch (state)
            {
                case PlayStates.Idle:
                    _record.IsEnabled = true;
                    if (_vm.AudioBuffer != null)
                        _play.IsEnabled = true;
                    else
                        _play.IsEnabled = false;
                    _stop.IsEnabled = false;
                    break;
                case PlayStates.Playing:
                    _record.IsEnabled = false;
                    _play.IsEnabled = false;
                    _stop.IsEnabled = true;
                    break;
                case PlayStates.Recording:
                    _record.IsEnabled = false;
                    _play.IsEnabled = false;
                    _stop.IsEnabled = true;
                    break;
                default:
                    break;
            }
        }
    }
}
