using DiversityPhone.ViewModels;
using Microsoft.Phone.Shell;
using System;

namespace DiversityPhone.View.Appbar {
    public class EditPageDeleteButton : CommandButtonAdapter {
        public EditPageDeleteButton(IApplicationBar appbar, IDeletePageVM vm)
            : base(appbar: appbar,
            button: new ApplicationBarIconButton() { IconUri = new Uri("/Images/appbar.delete.rest.png", UriKind.Relative), Text = "delete" },
            command: vm.Delete) {
        }
    }
}
