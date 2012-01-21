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

namespace DiversityPhone.View
{
    public class EditPageAppBarUpdater<T> where T : IModifyable
    {
        IApplicationBar _appbar;
        EditElementPageVMBase<T> _vm;
        ApplicationBarIconButton _save,_edit,_delete;

        public EditPageAppBarUpdater(IApplicationBar appbar, EditElementPageVMBase<T> viewmodel )
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

            _vm.ObservableForProperty(x => x.IsEditable)
                .Select(change => change.Value)                
                .Subscribe(iseditable => adjustApplicationBar(iseditable));

            _vm.Save.CanExecuteObservable
                .Subscribe(canSave => _save.IsEnabled = canSave);

            adjustApplicationBar(_vm.IsEditable);
                
        }

        private void adjustApplicationBar(bool editable)
        {
            {
                _appbar.Buttons.Clear();
                if (editable == true)
                {                    
                    _appbar.Buttons.Add(_save);
                    _appbar.Buttons.Add(_delete);
                }
                else
                {
                    _appbar.Buttons.Add(_edit);
                }
            }
        }


    }
}
