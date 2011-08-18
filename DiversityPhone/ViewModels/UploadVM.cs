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

namespace DiversityPhone.ViewModels
{
    public class UploadVM : ReactiveObject
    {
        INavigationService _navigation;
        IOfflineStorage _storage;
        IDiversityService _repository;
        IConnectivityService _connectivity;



        public UploadVM()
        {

        }
    }
}
