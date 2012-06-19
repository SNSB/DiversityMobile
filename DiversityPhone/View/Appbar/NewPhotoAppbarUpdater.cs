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

namespace DiversityPhone.View
{
    public class NewImageAppBarUpdater
    {
        IApplicationBar _appbar;
        NewImageVM _vm;
        ApplicationBarIconButton _take,_refresh, _save,_settings;//_crop

        public NewImageAppBarUpdater(IApplicationBar appbar, NewImageVM viewmodel)
        {
            _appbar = appbar;
            _vm = viewmodel;

            if (_vm == null)
                return;

            
            _take = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.feature.camera.rest.png",UriKind.Relative),
                Text = "take",
                IsEnabled = true,
            };
            _take.Click += (s,args) => _vm.Take.Execute(null);

             _refresh = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.refresh.rest.png",UriKind.Relative),
                Text = "refresh",
                IsEnabled = true,
            };
            _refresh.Click += (s,args) => _vm.Reset.Execute(null);

            //_crop = new ApplicationBarIconButton()
            //{
            //    IconUri = new Uri("/Images/appbar.edit.rest.png", UriKind.Relative),
            //    Text = "crop",
            //    IsEnabled = true,
            //};
            //_crop.Click += (s, args) => _vm.Crop.Execute(null);

             _save = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.save.rest.png",UriKind.Relative),
                Text = "save",
                IsEnabled = true,
            };
            _save.Click += (s,args) => _vm.Save.Execute(null);



            _settings= new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.feature.settings.rest.png", UriKind.Relative),
                Text = "settings",
                IsEnabled = true,
            };
            //Todo:Navigation

            _appbar.Buttons.Add(_take);
            _appbar.Buttons.Add(_refresh);
            _appbar.Buttons.Add(_save);
            //_appbar.Buttons.Add(_settings); //For Preparation of a setting page

            _vm.ObservableForProperty(x => x.ShootingEnabled)
              .Value()
              .StartWith(_vm.ShootingEnabled)
              .Subscribe(shootenabled=> adjustApplicationBar(shootenabled));
        }

        public void adjustApplicationBar(bool shootEnabled)
        {
            if (shootEnabled)
            {
                _take.IsEnabled=true;
                //_crop.IsEnabled=false;
                _refresh.IsEnabled=false;
                _save.IsEnabled=false;
            }
            else
            {
                _take.IsEnabled=true;
               // _crop.IsEnabled=true;
                if (_vm.OldImage != null)
                    _refresh.IsEnabled = true;
                else
                    _refresh.IsEnabled = false;
                if (_vm.ActualImage != null)
                    _save.IsEnabled = true;
                else
                    _save.IsEnabled = false;
            }
        }

    }
}
