using System;
using System.Net;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Services;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;


namespace DiversityPhone.ViewModels
{
    public class HomeVM : ReactiveObject
    {
        

        public ReactiveCommand Edit { get; private set; }
        public ReactiveCommand Settings { get; private set; }
        public ReactiveCommand Download { get; private set; }
        public ReactiveCommand Upload { get; private set; }

        private INavigationService Navigation { get; set; }
      

        public HomeVM(INavigationService nav, IMessageBus messenger)
        {
            Navigation = nav;

            (Edit = new ReactiveCommand())
                .Subscribe(_ => Navigation.Navigate(Services.Page.ListEventSeries));

            (Settings = new ReactiveCommand())
                .Subscribe(_ => Navigation.Navigate(Services.Page.Settings));

            (Download = new ReactiveCommand())
                .Subscribe(_ => Navigation.Navigate(Services.Page.ListEventSeries));

            (Upload = new ReactiveCommand())
                .Subscribe(_ => Navigation.Navigate(Services.Page.Upload));       
        }
    }
}
