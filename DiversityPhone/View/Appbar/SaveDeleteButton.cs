using DiversityPhone.ViewModels;
using Microsoft.Phone.Shell;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace DiversityPhone.View.Appbar
{
    public class SaveDeleteButton : SwitchingCommandButtonAdapter
    {
        private readonly ButtonState SAVE, DELETE;
        private IDisposable _subscription;

        public SaveDeleteButton(IApplicationBar appbar, IEditPageVM vm)
            : base(appbar)
        {
            SAVE = new ButtonState()
            {
                URI = new Uri("/Images/appbar.save.rest.png", UriKind.Relative),
                Text = DiversityResources.Button_Save,
                Command = vm.Save
            };

            DELETE = new ButtonState()
            {
                URI = new Uri("/Images/appbar.delete.rest.png", UriKind.Relative),
                Text = DiversityResources.Button_Delete,
                Command = vm.Delete
            };

            _subscription =
            vm.ObservableForProperty(x => x.IsEditable)
            .Value()
            .StartWith(vm.IsEditable)
            .Select(editable => (editable) ? SAVE : DELETE)
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