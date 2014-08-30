namespace DiversityPhone.View.Setup
{
    using DiversityPhone.ViewModels;
    using Microsoft.Phone.Controls;
    using Ninject;
    using System.Windows;

    public partial class Splash : PhoneApplicationPage
    {
        private bool Initialized = false;

        public Splash()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            InititializeIfNecessary();
        }

        private void InititializeIfNecessary()
        {
            if (!Initialized)
            {
                App.KernelInitialized += ActivateSetup;
                Initialized = true;

                App.StartInitialize();
            }
        }

        private void ActivateSetup()
        {
            var setup = App.Kernel.Get<SetupVM>();

            setup.Activate();
            App.KernelInitialized -= ActivateSetup;
        }
    }
}