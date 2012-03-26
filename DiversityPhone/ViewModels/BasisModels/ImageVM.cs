using System;
using System.Net;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using DiversityPhone.Model;
using DiversityPhone.Services;
using System.IO.IsolatedStorage;
using System.IO;
using ReactiveUI;

namespace DiversityPhone.ViewModels
{
    public class ImageVM : MultimediaObjectVM
    {
        private BitmapImage _thumb=null;

        public BitmapImage Thumb
        {
            get
            {
                if (_thumb != null)
                    return _thumb;
                else
                {
                    _thumb=LoadThumb();
                    return _thumb;
                }
            }
        }

        private BitmapImage LoadThumb()
        {

            BitmapImage bi = new BitmapImage();
            if (Model.MediaType != MediaType.Image)
                return null;

            byte[] data;

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isfs = isf.OpenFile(Model.Uri, FileMode.Open, FileAccess.Read))
                {

                    // Allocate an array large enough for the entire file
                    data = new byte[isfs.Length];
                    // Read the entire file and then close it
                    isfs.Read(data, 0, data.Length);
                    isfs.Close();
                }

            }
            // Create memory stream and bitmap
            MemoryStream ms = new MemoryStream(data);
            bi.SetSource(ms);
            return bi;
        }

         public ImageVM(IMessageBus _messenger, MultimediaObject model, Page targetPage)
            : base(_messenger, model, targetPage)
        {

        }

    }
}

