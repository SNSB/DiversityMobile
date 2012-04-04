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
using DiversityPhone.PMService;
using System.IO;
using ReactiveUI.Xaml;
using System.Windows.Media.Imaging;


namespace DiversityPhone.ViewModels
{
    public class ViewDownloadMapsVM : PageViewModel
    {
        #region Services

        private IFieldDataService _storage;
        private PhoneMediaServiceClient _mapinfo;
        private HttpWebRequest _imageHttp;

        #endregion


        #region Commands

        public ReactiveCommand Search { get; private set; }
        public ReactiveCommand Add { get; private set; }


        #endregion

        #region Properties

        private ObservableCollection<String> _AvailableMaps;

        public IList<String> AvailableMaps
        {
            get { return _AvailableMaps; }
        }

        public bool IsBusy { get { return _IsBusy.Value; } }
        private ObservableAsPropertyHelper<bool> _IsBusy;

        
        private ObservableAsPropertyHelper<IList<String>> _Keys;
        public IList<String> Keys
        {
            get { return _Keys.Value; }
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

        public ViewDownloadMapsVM(IFieldDataService storage)  
        {
            _storage = storage;


            Search = new ReactiveCommand(_IsBusy.Select(x => !x))
                .Subscribe(searchText => searchMaps(searchText));
       
            
            Add = new ReactiveCommand(_IsBusy.Select(x => !x))
                    .Subscribe(mapID=> addMap(mapID));


            _Keys = StateObservable
                .Select(_ => getStorageList())
                .ToProperty(this, x => Keys);

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
            string keyTrunk = key.Substring(0, key.LastIndexOf(".") - 1);
            string keyPng=keyTrunk + ".png";
            string keyXML=keyTrunk+".xml";
            if (Keys.Contains(keyXML)&&Keys.Contains(keyPng))
            {
                Map newMap = Map.loadMapParameterFromFile(keyXML);
                newMap.Uri = keyPng;
                saveMap(newMap);
                Keys.Remove(keyXML);
                Keys.Remove(keyPng);
                loadMap(keyPng);
            }
                      
        }

        public void saveMap(Map map)
        {
            _storage.addOrUpdateMap(map);
        }

        private void loadMap(String filename)
        {

            byte[] data;

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {

                using (IsolatedStorageFileStream isfs = isf.OpenFile(filename, FileMode.Open, FileAccess.Read))
                {
                    data = new byte[isfs.Length];
                    isfs.Read(data, 0, data.Length);
                    isfs.Close();
                }

            }
            MemoryStream ms = new MemoryStream(data);
            BitmapImage bi = new BitmapImage();
            bi.SetSource(ms);
            MapImage = bi;
        }

        #endregion

        #region Maplist Process

        private void searchMaps(string substring)
        {
            _mapinfo.GetMapListAsync(substring);
        }

        public void mapinfo_GetMapListCompleted(object sender, GetMapListFilterCompletedEventArgs e)
        {
            _AvailableMaps = e.Result;
        }

        #endregion

        #region Download Process

        //1. Select Map and download corresponding url
        private void addMap(String mapID)
        {
            //Get correponding URL for the map for the download from the SNSB IT-Center
            _mapinfo.GetMapUrlAsync(mapID);
            _mapinfo.GetXmlUrlAsync(mapID);
        }

        //2. Initiate DownloadMap
        public void mapinfo_GetMapUrlCompleted(object sender, GetMapUrlCompletedEventArgs e)
        {
            //The Result of the selection is passed in the Arguments of the event
            string transferFileName = e.Result;
            Uri transferUri = new Uri(Uri.EscapeUriString(transferFileName), UriKind.RelativeOrAbsolute);

            _imageHttp = (HttpWebRequest)WebRequest.CreateHttp(transferUri);
            string credentials = Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes("snsb" + ":" + "maps"));
            _imageHttp.Headers["Authorization"] = "Basic " + credentials;
            _imageHttp.BeginGetResponse(DownloadCallback, _imageHttp);
            //Todo put DownloadProcess on the UI
        }


        //3.Get CorrespondingMapData
        public void mapinfo_GetXmlUrlCompleted(object sender, GetXmlUrlCompletedEventArgs e)
        {
            string transferFileName = e.Result;
            Uri transferUri = new Uri(Uri.EscapeUriString(transferFileName), UriKind.RelativeOrAbsolute);

            _imageHttp = (HttpWebRequest)WebRequest.CreateHttp(transferUri);
            string credentials = Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes("snsb" + ":" + "maps"));
            _imageHttp.Headers["Authorization"] = "Basic " + credentials;
            _imageHttp.BeginGetResponse(DownloadCallback, _imageHttp);
        }

        //4. Save Files and Add to KeyList
        private void DownloadCallback(IAsyncResult result)
        {
            HttpWebRequest req1 = (HttpWebRequest)result.AsyncState;
            HttpWebResponse response = (HttpWebResponse)req1.EndGetResponse(result);
            Stream receiveStream = response.GetResponseStream();
            String uriName = req1.RequestUri.OriginalString;
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
