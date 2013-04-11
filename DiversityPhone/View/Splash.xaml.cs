
using DiversityPhone.Services;
using ReactiveUI;
using Microsoft.Phone.Controls;
using System.Windows;
using DiversityPhone.Interface;
using DiversityPhone.Model;
using Ninject;
using System.Reactive.Linq;

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
            var settings = App.Kernel.Get<ISettingsService>().CurrentSettings().FirstOrDefault();
            var messenger = App.Kernel.Get<IMessageBus>();
            if (settings == null)
                messenger.SendMessage(Page.Setup);
            else
                messenger.SendMessage(Page.Home);
        }
    }
}