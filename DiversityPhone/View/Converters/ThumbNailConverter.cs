using Ninject;
using ReactiveUI;
using System;
using System.Windows.Data;
using System.Windows.Media;

namespace DiversityPhone.View {
    public class ThumbNailConverter : IValueConverter {
        IStoreImages _ImageStore;
        IStoreImages ImageStore {
            get {
                if (_ImageStore == null) {
                    _ImageStore = App.Kernel.Get<IStoreImages>();
                }
                return _ImageStore;
            }
        }

        const int MAX_THUMBNAILS = 100;

        private MemoizingMRUCache<string, ImageSource> thumbCache;

        private ImageSource loadThumb(string uri, object _) {
            if (!string.IsNullOrEmpty(uri)) {
                return ImageStore.GetImageThumbnail(uri);
            }
            return null;
        }

        public ThumbNailConverter() {

            thumbCache = new MemoizingMRUCache<string, ImageSource>(loadThumb, MAX_THUMBNAILS);

        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string imagePath = value as string;

            if (imagePath != null) {
                return thumbCache.Get(imagePath);
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return null;
        }
    }
}
