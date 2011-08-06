using System;
using System.Net;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Services;
using System.Linq;
using System.Reactive.Linq;


namespace DiversityPhone.ViewModels
{
    public class HomeViewModel : ReactiveObject
    {
        

        public ReactiveCommand Edit { get; private set; }
        public ReactiveCommand Settings { get; private set; }
        public ReactiveCommand Download { get; private set; }

        private INavigationService Navigation { get; set; }
      

        public HomeViewModel(INavigationService nav)
        {
            Navigation = nav;

            Edit = new ReactiveCommand();
            Edit.Subscribe(_ => Navigation.Navigate(Services.Page.EventSeries));

            Settings = new ReactiveCommand();
            Settings.Subscribe(_ => Navigation.Navigate(Services.Page.Settings));
        }
    }
}
