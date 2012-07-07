using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using DiversityPhone.Model;
using DiversityPhone.ViewModels;
using System.IO;
using DiversityPhone.MapMediaService;
using DiversityPhone.Services;
using System.Collections.ObjectModel;

namespace DiversityPhone.View
{
    public partial class ViewDLM : PhoneApplicationPage
    {

        private bool _isBusy;
        private ViewDLMVM VM { get { return DataContext as ViewDLMVM; } }


        #region Services

     
        private PhoneMediaServiceClient _mapinfo;
        private HttpWebRequest _imageHttp;

        #endregion

        #region Properties

        //private ObservableCollection<String> _AvailableMaps;

        //public IList<String> AvailableMaps
        //{
        //    get { return _AvailableMaps; }
        //}


        private IList<String> _Keys;
        public IList<String> Keys
        {
            get { return _Keys; }
        }


        #endregion

       

        public ViewDLM()
        {
            InitializeComponent();
    
            _mapinfo = new PhoneMediaServiceClient();
            _mapinfo.GetMapListFilterCompleted += new EventHandler<GetMapListFilterCompletedEventArgs>(mapinfo_GetMapListCompleted);
            _mapinfo.GetMapUrlCompleted += new EventHandler<GetMapUrlCompletedEventArgs>(mapinfo_GetMapUrlCompleted);
            _mapinfo.GetXmlUrlCompleted += new EventHandler<GetXmlUrlCompletedEventArgs>(mapinfo_GetXmlUrlCompleted);
            _Keys = new List<String>();
        }

        #region UI-Events
        private void mapText_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            String s = ((TextBlock)sender).Tag as string;
            //TODO
            //Show Details of the Map-Requires XML-Data for the Map
        }

        private void btn_Search_Click(object sender, RoutedEventArgs e)
        {

            if (textBoxSearch.Text.Length > 2)
            {
                _mapinfo.GetMapListFilterAsync(textBoxSearch.Text);
            }
            else
                MessageBox.Show("Minimum lenght for search is 3");
        }


        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = this._isBusy;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            this.addMap(((Button)sender).Tag as string);
        }

        #endregion



        #region Helper

        private IList<String> getStorageList()
        {
            return new List<String>();
        }

        public void saveWhenKeysArePresent(string key)
        {
            Keys.Add(key);
            string keyTrunk = key.Substring(0, key.LastIndexOf("."));
            string keyPng = keyTrunk + ".png";
            string keyXML = keyTrunk + ".xml";
            if (Keys.Contains(keyXML) && Keys.Contains(keyPng))
            {
                Map newMap = Map.loadMapParameterFromFile(keyXML);
                newMap.Uri = keyPng;
                saveMap(newMap);
                Keys.Remove(keyXML);
                Keys.Remove(keyPng);
                //loadMap(keyPng);
                _isBusy = false;
            }
           
        }

        public void saveMap(Map map)
        {
            VM.Save.Execute(map);
        }

        //private void loadMap(String filename)
        //{

        //    byte[] data;

        //    using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
        //    {

        //        using (IsolatedStorageFileStream isfs = isf.OpenFile(filename, FileMode.Open, FileAccess.Read))
        //        {
        //            data = new byte[isfs.Length];
        //            isfs.Read(data, 0, data.Length);
        //            isfs.Close();
        //        }

        //    }
        //    MemoryStream ms = new MemoryStream(data);
        //    BitmapImage bi = new BitmapImage();
        //    bi.SetSource(ms);
        //    MapImage.Source = bi;
        //}

        #endregion

        #region Load Available Maps

        public void mapinfo_GetMapListCompleted(object sender, GetMapListFilterCompletedEventArgs e)
        {
            VM.AvailableMaps.Clear();
            foreach (String map in e.Result)
                VM.AvailableMaps.Add(map);
        }

        #endregion

        #region Download Process

        //1. Select Map and download corresponding url
        private void addMap(String mapName)
        {
            if (_isBusy == true)
                return;
            _isBusy = true;
            //Get correponding URL for the map for the download from the SNSB IT-Center
            _mapinfo.GetMapUrlAsync(mapName);
            _mapinfo.GetXmlUrlAsync(mapName);
        }

        //2. Initiate DownloadMap
        public void mapinfo_GetMapUrlCompleted(object sender, GetMapUrlCompletedEventArgs e)
        {
            //Todo: Check if File is present


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