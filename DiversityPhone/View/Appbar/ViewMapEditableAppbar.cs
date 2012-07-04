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
using DiversityPhone.ViewModels.Maps;
using Microsoft.Phone.Shell;
using DiversityPhone.ViewModels;
using System.Reactive.Linq;
using ReactiveUI;

namespace DiversityPhone.View
{
    public class ViewMapEditableAppbar
    {
        IApplicationBar _appbar;
        ViewMapEditVM _vm;
        ApplicationBarIconButton _save,_edit,_delete, _reset;

        public ViewMapEditableAppbar(IApplicationBar appbar, ViewMapEditVM viewmodel )
        {
            _appbar = appbar;
            _vm = viewmodel;

            if (_vm == null)
                return;

            _save = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.save.rest.png",UriKind.Relative),
                Text = "save",
                IsEnabled = true,
            };
            _save.Click += (s,args) => _vm.Save.Execute(null);
            _edit = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.edit.rest.png",UriKind.Relative),
                Text = "edit",
                IsEnabled = true,
            };
            _edit.Click += (s,args) => _vm.ToggleEditable.Execute(null);
            _delete = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.delete.rest.png",UriKind.Relative),
                Text = "delete",
                IsEnabled = true,
            };
            _delete.Click += (s,args) => _vm.Delete.Execute(null);

            _reset = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.refresh.rest.png", UriKind.Relative),
                Text = "refresh",
                IsEnabled = true,
            };
            _reset.Click += (s, args) => _vm.Reset.Execute(null);


            _vm.ObservableForProperty(x => x.IsEditable)
                .Value()
                .StartWith(_vm.IsEditable)
                .Subscribe(iseditable => adjustApplicationBar(iseditable));

            _vm.Save.CanExecuteObservable
                .StartWith(_vm.Save.CanExecute(null))
                .Subscribe(canSave => _save.IsEnabled = canSave);               
        }

        private void adjustApplicationBar(bool editable)
        {
            {
                _appbar.Buttons.Clear();
                if (editable == true)
                {                    
                    _appbar.Buttons.Add(_save);
                    _appbar.Buttons.Add(_delete);
                    _appbar.Buttons.Add(_reset);
                }
                else
                {
                    _appbar.Buttons.Add(_edit);
                }
            }
        }

    }
}

