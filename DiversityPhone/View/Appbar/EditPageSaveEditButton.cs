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
    public class EditPageSaveEditButton : SwitchingCommandButtonAdapter, IDisposable
    {
        private readonly ButtonState SAVE, EDIT;
        private IDisposable _subscription;

        public EditPageSaveEditButton(IApplicationBar appbar, ISavePageVM vm)
            : base(appbar)
        {
            SAVE = new ButtonState()
            {
                URI = new Uri("/Images/appbar.save.rest.png", UriKind.Relative),
                Text = DiversityResources.Button_Save,
                Command = vm.Save
            };

            EDIT = new ButtonState()
            {
                URI = new Uri("/Images/appbar.edit.rest.png", UriKind.Relative),
                Text = DiversityResources.Button_Edit,
                Command = vm.ToggleEditable
            };

            _subscription = 
            vm.ObservableForProperty(x => x.IsEditable)
                .Value()
                .StartWith(vm.IsEditable)
                .Select(editable => (editable) ? SAVE : EDIT)
                .Subscribe(state => this.CurrentState = state);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _subscription.Dispose();
            base.Dispose(disposing);
        }
    }
}
