namespace DiversityPhone.View.Setup {
    using DiversityPhone.View.Appbar;
    using DiversityPhone.ViewModels;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Tasks;
    using System;
    using System.Linq;
    using System.Windows;

    public partial class Welcome : PhoneApplicationPage {
        private OKNextPageCommandButton OKNext;

        private SetupVM VM { get { return DataContext as SetupVM; } }

        public Welcome() {
            InitializeComponent();

            OKNext = new OKNextPageCommandButton(VM.ShowLogin);
            this.ApplicationBar.Buttons.Add(OKNext.Button);
        }

        private void ClearBackStack() {
            while (this.NavigationService.BackStack.Any())
                this.NavigationService.RemoveBackEntry();
        }

        private void PhoneApplicationPage_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            ClearBackStack();
        }

        private void MapWiki_Click(object sender, RoutedEventArgs e) {
            new WebBrowserTask()
            {
                Uri = new Uri(DiversityResources.App_Map_URL, UriKind.Absolute)
            }.Show();
        }

        private void TaxWiki_Click(object sender, RoutedEventArgs e) {
            new WebBrowserTask()
            {
                Uri = new Uri(DiversityResources.App_Taxa_URL, UriKind.Absolute)
            }.Show();
        }

        private void Mail_Click(object sender, RoutedEventArgs e) {
            new EmailComposeTask()
            {
                To = DiversityResources.App_Mail_Address,
                Subject = "DiversityMobile"
            }.Show();
        }

        private void Homepage_Click(object sender, RoutedEventArgs e) {
            new WebBrowserTask()
            {
                Uri = new Uri(DiversityResources.App_Homepage_URL, UriKind.Absolute)
            }.Show();
        }

    }
}