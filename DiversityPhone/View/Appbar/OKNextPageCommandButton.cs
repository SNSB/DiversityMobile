using Microsoft.Phone.Shell;
using System;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace DiversityPhone.View.Appbar {
    public class OKNextPageCommandButton : CommandButtonAdapter {

        public OKNextPageCommandButton(ICommand Command)
            : base(new ApplicationBarIconButton(new Uri("/Images/appbar.check.rest.png", UriKind.Relative)) { Text = DiversityResources.Button_OK }, Command) {
            Contract.Requires(Command != null);
        }
    }
}
