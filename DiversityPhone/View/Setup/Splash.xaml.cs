namespace DiversityPhone.View.Setup {
    using DiversityPhone.ViewModels;
    using Microsoft.Phone.Controls;
    using Ninject;
    using System.Windows;

    public partial class Splash : PhoneApplicationPage {
        private bool Initialized = false;

        public Splash() {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e) {
            InititializeIfNecessary();
            ActivateSetup();
        }

        private void InititializeIfNecessary() {
            if (!Initialized) {
                Initialized = true;

                App.Initialize();
            }
        }

        private void ActivateSetup() {
            var setup = App.Kernel.Get<SetupVM>();

            setup.Activate();
        }
    }
}