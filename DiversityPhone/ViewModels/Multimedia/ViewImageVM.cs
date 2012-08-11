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
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reactive.Linq;


namespace DiversityPhone.ViewModels
{
    public class ViewImageVM : EditPageVMBase<MultimediaObject>
    {
 
        #region Properties
        private string _Uri;
        public string Uri
        {
            get { return _Uri; }
            set { this.RaiseAndSetIfChanged(x => x.Uri, ref _Uri, value); }
        }

        private BitmapImage _savedImage;

        public BitmapImage SavedImage
        {
            get
            {
                return _savedImage;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SavedImage, ref _savedImage, value);
            }
        }
        #endregion

        public ViewImageVM()  
            : base(mmo => mmo.MediaType == MediaType.Image)
        {


        }

        private void LoadImage(MultimediaObject mmo)
        {

            if (mmo.MediaType != MediaType.Image)
                return;


            byte[] data;

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {

                using (IsolatedStorageFileStream isfs = isf.OpenFile(mmo.Uri, FileMode.Open, FileAccess.Read))
                {
                    data = new byte[isfs.Length];
                    isfs.Read(data, 0, data.Length);
                    isfs.Close();
                }

            }
            MemoryStream ms = new MemoryStream(data);
            BitmapImage bi = new BitmapImage();
            bi.SetSource(ms);
            SavedImage = bi;
        }
    }
}
