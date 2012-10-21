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

namespace DiversityPhone.View.Appbar
{
    public sealed class EditPageSaveEditButton : IDisposable
    {
        private readonly Uri SAVE_IMAGE = new Uri("/Images/appbar.save.rest.png", UriKind.Relative);
        private readonly Uri EDIT_IMAGE = new Uri("/Images/appbar.edit.rest.png", UriKind.Relative);
        private readonly string SAVE_TEXT;
        private readonly string EDIT_TEXT;

        IApplicationBarIconButton _btn;
        ISavePageVM _vm;
        CommandButtonAdapter _cmdadapter;

        public EditPageSaveEditButton(IApplicationBar appbar, ISavePageVM vm)
            : this(new ApplicationBarIconButton() { IsEnabled = true }, vm)
        {
            if(appbar == null)
                throw new ArgumentNullException("appbar");
            
            appbar.Buttons.Add(_cmdadapter.Button);
        }

        public EditPageSaveEditButton(IApplicationBarIconButton btn, ISavePageVM vm)
        {
            _btn = btn;
            _vm = vm;

            if (_vm == null)
                throw new ArgumentNullException("vm");
            if (_btn == null)
                throw new ArgumentNullException("btn");

            SAVE_TEXT = "save";
            EDIT_TEXT = "edit";

            _cmdadapter = new CommandButtonAdapter(_btn);

            _vm.ObservableForProperty(x => x.IsEditable)
                .Value()
                .StartWith(_vm.IsEditable)
                .Subscribe(iseditable => adjust_button(iseditable));                     
        }

        private void adjust_button(bool iseditable)
        {
            _cmdadapter.Command = (iseditable) ? _vm.Save : _vm.ToggleEditable;
            _btn.IconUri = (iseditable) ? SAVE_IMAGE : EDIT_IMAGE;
            _btn.Text = (iseditable) ? SAVE_TEXT : EDIT_TEXT;
        }


        public void Dispose()
        {
            _cmdadapter.Dispose();
            _btn = null;
            _vm = null;
            _cmdadapter = null;
        }
    }
}
