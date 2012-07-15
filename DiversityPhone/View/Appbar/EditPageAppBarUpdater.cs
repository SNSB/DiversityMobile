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
using DiversityPhone.View.Appbar;

namespace DiversityPhone.View
{
    public class EditPageAppBarUpdater<T> where T : IModifyable
    {
        IApplicationBar _appbar;
        EditElementPageVMBase<T> _vm;
        ApplicationBarIconButton _save,_edit,_delete;
        CommandButtonAdapter _to_save, _to_edit, _to_delete;

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
            _to_save = new CommandButtonAdapter(_vm.Save, _save);
            _edit = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.edit.rest.png",UriKind.Relative),
                Text = "edit",
                IsEnabled = true,
            };
            _to_edit = new CommandButtonAdapter(_vm.ToggleEditable, _edit);
            _delete = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Images/appbar.delete.rest.png",UriKind.Relative),
                Text = "delete",
                IsEnabled = true,
            };
            _to_delete = new CommandButtonAdapter(_vm.Delete, _delete);            

            _vm.ObservableForProperty(x => x.IsEditable)
                .Value()
                .StartWith(_vm.IsEditable)
                .Subscribe(iseditable => adjustApplicationBar(iseditable));                     
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
                    _appbar.Buttons.Add(_delete);
                }
            }
        }

    }
}
