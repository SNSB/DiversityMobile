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
using ReactiveUI;
using DiversityPhone.Services;
using DiversityPhone.Service;
using ReactiveUI.Xaml;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public class SetupVM : ReactiveObject
    {
        INavigationService _navigation;
        IOfflineStorage _storage;
        IDiversityService _repository;
        IConnectivityService _connectivity;

        public ReactiveCommand AcceptUserData { get; private set; }

        public SetupVM(INavigationService nav, IOfflineStorage storage, IDiversityService repo)//, IConnectivityService conn)
        {
            _navigation = nav;
            _storage = storage;
            _repository = repo;
            //_connectivity = conn;

            (AcceptUserData = new ReactiveCommand())
                .Subscribe(_ => acceptUserData());
        }

        private void acceptUserData()
        {
            downloadVocabulary();
        }

        private void downloadVocabulary()
        {
            
        }
    }
}
