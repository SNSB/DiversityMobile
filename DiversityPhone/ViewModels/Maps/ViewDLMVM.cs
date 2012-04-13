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
using DiversityPhone.Services;
using System.IO.IsolatedStorage;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using ReactiveUI;
using System.Windows.Media.Imaging;
using DiversityPhone.Model;
using ReactiveUI.Xaml;

namespace DiversityPhone.ViewModels
{
    public class ViewDLMVM :PageViewModel
    {

        private IMapStorageService _mapStorage;

        #region Commands

        public ReactiveCommand Save { get; private set; }

        #endregion
        #region Properties

        private ObservableCollection<String> _AvailableMaps;

        public IList<String> AvailableMaps
        {
            get { return _AvailableMaps; }
  
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
        }


        private ObservableAsPropertyHelper<IList<String>> _Keys;
        public IList<String> Keys
        {
            get { return _Keys.Value; }
        }


        private String _SearchString;
        public String SearchString
        {
            get { return _SearchString; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SearchString, ref _SearchString, value);
            }
        }


        private BitmapImage _mapImage;

        public BitmapImage MapImage
        {
            get
            {
                return _mapImage;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.MapImage, ref _mapImage, value);
            }
        }

        #endregion


        public ViewDLMVM(IMapStorageService mapStorage)  
        {

            _mapStorage = mapStorage;
            Save = new ReactiveCommand();
            Save.Subscribe(mapName => saveMap(mapName as Map));

            _AvailableMaps = new ObservableCollection<string>();
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("Maps"))
                {
                    isoStore.CreateDirectory("Maps");
                }
            }
          
        }


        private void saveMap(Map map)
        {
            _mapStorage.addOrUpdateMap(map);
        }
    }
}
