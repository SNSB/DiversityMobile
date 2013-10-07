using DiversityPhone.ViewModels;
using Microsoft.Phone.Shell;
using ReactiveUI;
using System;
using System.Reactive.Linq;

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
