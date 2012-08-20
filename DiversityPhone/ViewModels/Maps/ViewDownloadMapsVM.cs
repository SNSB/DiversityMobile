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
using DiversityPhone.Messages;
using DiversityPhone.Model;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.IO.IsolatedStorage;
using DiversityPhone.MapMediaService;
using System.IO;
using ReactiveUI.Xaml;
using System.Windows.Media.Imaging;
using System.Reactive.Subjects;


namespace DiversityPhone.ViewModels
{
    public class ViewDownloadMapsVM : PageVMBase
    {
        #region Services

        private IMapStorageService _mapStorage;
        private PhoneMediaServiceClient _mapinfo;     

        #endregion


        #region Commands

        public ReactiveAsyncCommand Search { get; private set; }
        public ReactiveCommand Add { get; private set; }


        #endregion

        #region Properties

        private String _mapName;

        private ObservableCollection<String> _AvailableMaps;
        public IList<String> AvailableMaps
        {
            get { return _AvailableMaps; }
        }

        public bool IsBusy 
        { 
            get { return _IsBusy.Value; }    
        }
        private ObservableAsPropertyHelper<bool> _IsBusy;
        private ISubject<bool> _isBusySubject = new Subject<bool>();

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

        public ViewDownloadMapsVM(IMapStorageService mapStorage)  
        {
            _mapStorage = mapStorage;
            _AvailableMaps = new ObservableCollection<string>();

            Search = new ReactiveAsyncCommand();
            Search.Subscribe(_=>searchMaps());

            _IsBusy = this.ObservableToProperty(_isBusySubject, x=> x.IsBusy);

            Add = new ReactiveCommand();
            Add.Subscribe(mapName => addMap(mapName as String));

            _Keys = this.ObservableToProperty(
                ActivationObservable.Where(activated => activated)
                .Select(_ => getStorageList()),
                x => x.Keys);

            _mapinfo = new PhoneMediaServiceClient();
            _mapinfo.GetMapListFilterCompleted += new EventHandler<GetMapListFilterCompletedEventArgs>(mapinfo_GetMapListCompleted);
            _mapinfo.GetMapUrlCompleted += new EventHandler<GetMapUrlCompletedEventArgs>(mapinfo_GetMapUrlCompleted);
            _mapinfo.GetXmlUrlCompleted += new EventHandler<GetXmlUrlCompletedEventArgs>(mapinfo_GetXmlUrlCompleted);
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("Maps"))
                {
                    isoStore.CreateDirectory("Maps");
                }
            }
          
        }

        #region Helper

        private IList<String> getStorageList()
        {
            return new List<String>();
        }

        public void saveWhenKeysArePresent(string key)
        {
            Keys.Add(key);
            string keyTrunk = key.Substring(0, key.LastIndexOf("."));
            string keyPng=keyTrunk + ".png";
            string keyXML=keyTrunk+".xml";
            if (Keys.Contains(keyXML)&&Keys.Contains(keyPng))
            {
                Map newMap = Map.loadMapParameterFromFile(keyXML);
                newMap.Uri = keyPng;
                newMap.ServerKey = _mapName;
                saveMap(newMap);
                Keys.Remove(keyXML);
                Keys.Remove(keyPng);
                _isBusySubject.OnNext(false);
                Messenger.SendMessage<DialogMessage>(new DialogMessage(DialogType.OK,"Ready","Ready"));
            }
                    
        }

        public void saveMap(Map map)
        {
            //_mapStorage.addOrUpdateMap(map);
        }

        #endregion

        #region Load Available Maps

        private void searchMaps()
        {
            _isBusySubject.OnNext(true);
            _mapinfo.GetMapListFilterAsync(SearchString);
        }

        public void mapinfo_GetMapListCompleted(object sender, GetMapListFilterCompletedEventArgs e)
        {
            AvailableMaps.Clear();
            _isBusySubject.OnNext(false);
            foreach (String map in e.Result)
                AvailableMaps.Add(map);
          
        }

        #endregion

        #region Download Process

        //1. Select Map and download corresponding url
        private void addMap(String serverKey)
        {
            if (IsBusy == true)
                return;
            _isBusySubject.OnNext(true);
            if (_mapStorage.isPresent(serverKey))
            {
                if (MessageBox.Show("Map is already present. Override?", "Map present", MessageBoxButton.OKCancel).Equals(MessageBoxResult.Cancel))
                {
                    _isBusySubject.OnNext(false);
                    return;
                }
            }


            //Get correponding URL for the map for the download from the SNSB IT-Center
            _mapinfo.GetMapUrlAsync(serverKey);
            _mapinfo.GetXmlUrlAsync(serverKey);
            _mapName = serverKey;
        }

        //2. Initiate DownloadMap
        public void mapinfo_GetMapUrlCompleted(object sender, GetMapUrlCompletedEventArgs e)
        {
         
            //The Result of the selection is passed in the Arguments of the event
            string transferFileName = e.Result;
            Uri transferUri = new Uri(Uri.EscapeUriString(transferFileName), UriKind.RelativeOrAbsolute);

            var _imageHttp = (HttpWebRequest)WebRequest.CreateHttp(transferUri);
            string credentials = Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes("snsb" + ":" + "maps"));
            _imageHttp.Headers["Authorization"] = "Basic " + credentials;
            Observable.FromAsyncPattern<WebResponse>(_imageHttp.BeginGetResponse, _imageHttp.EndGetResponse)
               .Invoke()
               .SubscribeOnDispatcher() //This allows us to modify UI state in callback
               .Subscribe(DownloadCallback);              
        }


        //3.Get CorrespondingMapData
        public void mapinfo_GetXmlUrlCompleted(object sender, GetXmlUrlCompletedEventArgs e)
        {
            string transferFileName = e.Result;
            Uri transferUri = new Uri(Uri.EscapeUriString(transferFileName), UriKind.RelativeOrAbsolute);

            var _imageHttp = (HttpWebRequest)WebRequest.CreateHttp(transferUri);
            string credentials = Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes("snsb" + ":" + "maps"));
            _imageHttp.Headers["Authorization"] = "Basic " + credentials;
            Observable.FromAsyncPattern<WebResponse>(_imageHttp.BeginGetResponse, _imageHttp.EndGetResponse)
                .Invoke()
                .SubscribeOnDispatcher() //This allows us to modify UI state in callback
                .Subscribe(DownloadCallback);            
        }

        //4. Save Files and Add to KeyList
        private void DownloadCallback(WebResponse response)
        {            
            Stream receiveStream = response.GetResponseStream();
            String uriName = response.ResponseUri.OriginalString; //Not sure if this works
            int index = uriName.LastIndexOf("/") + 1;
            String fileName = "Maps\\" + uriName.Substring(index, uriName.Length - index);
            int lenght = (int)response.ContentLength;
            StreamReader readStream = new StreamReader(receiveStream);
            byte[] contents;
            BinaryReader binread = new BinaryReader(receiveStream);
            contents = binread.ReadBytes(lenght);
            using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var isoFileStream = isoStore.CreateFile(fileName))
                {
                    isoFileStream.Write(contents, 0, contents.Length);
                    isoFileStream.Close();
                }
            }
            saveWhenKeysArePresent(fileName);
        }
        #endregion

    }
}
