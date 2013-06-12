using ReactiveUI;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ninject;

namespace DiversityPhone.View
{
    public class ThumbNailConverter : IValueConverter
    {
        IStoreImages _ImageStore;
        IStoreImages ImageStore
        {
            get
            {
                if (_ImageStore == null)
                {
                    _ImageStore = App.Kernel.Get<IStoreImages>();
                }
                return _ImageStore;
            }
        }

        const int THUMBNAIL_HEIGHT = 40,
                  THUMBNAIL_WIDTH = 40,
                  MAX_THUMBNAILS = 100;

        private MemoizingMRUCache<string, ImageSource> thumbCache;

        private ImageSource loadThumb(string uri, object _)
        {

            if (!string.IsNullOrEmpty(uri))
            {
                using (var thumbnailStream = ImageStore.GetImageThumbnail(uri))
                {
                    if (thumbnailStream.Length > 0)
                    {
                        var fullScaleImage = new BitmapImage();
                        fullScaleImage.SetSource(thumbnailStream);
                        var thumbnailBitmap = new WriteableBitmap(fullScaleImage).Resize(THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT, WriteableBitmapExtensions.Interpolation.NearestNeighbor);

                        return thumbnailBitmap;
                    }
                }
            }
            return null;
        }

        public ThumbNailConverter()
        {

            thumbCache = new MemoizingMRUCache<string, ImageSource>(loadThumb, MAX_THUMBNAILS);

        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string imagePath = value as string;

            if (imagePath != null)
            {
                return thumbCache.Get(imagePath);
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
