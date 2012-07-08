
using DiversityPhone.Services;
using ReactiveUI;
using Microsoft.Phone.Controls;
using System.Windows;

namespace DiversityPhone.View
{
    public partial class Splash : PhoneApplicationPage
    {


        public Splash()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.Initialize();
            var settings = App.IOC.Resolve<ISettingsService>().getSettings();
            var messenger = App.IOC.Resolve<IMessageBus>();
            if (settings == null)
                messenger.SendMessage(Page.Setup);
            else
                messenger.SendMessage(Page.Home);

        }
    }
}