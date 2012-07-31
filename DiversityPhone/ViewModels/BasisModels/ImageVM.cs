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
            return null;
        }

         public ImageVM(MultimediaObject model)
            : base(model)
        {

        }

    }
}

