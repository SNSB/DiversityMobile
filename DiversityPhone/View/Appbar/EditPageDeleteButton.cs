using DiversityPhone.ViewModels;
using Microsoft.Phone.Shell;
using System;

namespace DiversityPhone.View.Appbar
{
    public class EditPageDeleteButton : CommandButtonAdapter
    {
        public EditPageDeleteButton(IApplicationBar appbar, IDeletePageVM vm)
            : base(appbar, vm.Delete)
        {
            this.Button.IconUri = new Uri("/Images/appbar.delete.rest.png",UriKind.Relative);
            this.Button.Text = "delete";            
        }
    }
}
