using Microsoft.Phone.Controls;
using System.Linq;
using System.Windows;

namespace DiversityPhone.View.Setup
{
    public partial class Vocabulary : PhoneApplicationPage
    {
        public Vocabulary()
        {
            InitializeComponent();            
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            while (this.NavigationService.BackStack.Any())
                this.NavigationService.RemoveBackEntry();
        }
    }
}