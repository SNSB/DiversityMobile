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
    public class HomeViewModel : ReactiveObject
    {
        

        public ReactiveCommand Edit { get; private set; }
        public ReactiveCommand Settings { get; private set; }
        public ReactiveCommand Download { get; private set; }

        private INavigationService Navigation { get; set; }
      

        public HomeViewModel(INavigationService nav, IMessageBus messenger)
        {
            Navigation = nav;

            (Edit = new ReactiveCommand())
                .Subscribe(_ => Navigation.Navigate(Services.Page.EventSeries));

            (Settings = new ReactiveCommand())
                .Subscribe(_ => Navigation.Navigate(Services.Page.Settings));

            (Download = new ReactiveCommand())
                .Subscribe(_ => Navigation.Navigate(Services.Page.EventSeries));

            RxApp.MessageBus.Listen<int>("test")
                .Subscribe(i => MessageBox.Show(i.ToString()));

            RxApp.MessageBus.SendMessage<int>(42, "test");
        }
    }
}
