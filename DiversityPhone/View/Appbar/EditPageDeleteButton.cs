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

namespace DiversityPhone.View.Appbar
{
    public class EditPageDeleteButton : CommandButtonAdapter
    {
        public EditPageDeleteButton(IApplicationBar appbar, IEditPageVM vm)
            : base(
                new ApplicationBarIconButton() 
                {
                    IconUri = new Uri("/Images/appbar.delete.rest.png",UriKind.Relative),
                    Text = "delete",
                    IsEnabled = true,
                }, vm.Delete)
        {
            if (appbar == null)
                throw new ArgumentNullException("appbar");

            appbar.Buttons.Add(Button);
        }
    }
}
