
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;


namespace DiversityPhone.ViewModels
{
    public class MultimediaObjectVM : ReactiveObject, IElementVM<MultimediaObject>
    {  
        private static BitmapImage load_thumb(MultimediaObject mmo)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isfs = isf.OpenFile(mmo.Uri, FileMode.Open, FileAccess.Read))
                {
                    var res = new BitmapImage();
                    res.SetSource(isfs);
                    return res;
                }
            }
        }
        private static ObservableAsyncMRUCache<MultimediaObject, BitmapImage> thumbnails = new ObservableAsyncMRUCache<MultimediaObject, BitmapImage>(mmo => Observable.Start(() => load_thumb(mmo)), 10);
        

        public MultimediaObject Model
        {
            get;
            private set;
        }

        public string Description { get { return Model.MediaType.ToString(); } }

        //General Implementation
        public Icon Icon
        {
            get
            {
                switch (Model.MediaType)
                {
                    case MediaType.Image:
                        return Icon.Photo;
                    case MediaType.Audio:
                        return Icon.Audio;
                    case MediaType.Video:
                        return Icon.Video;
                    default:
                        return Icon.Photo;
                }
            }
        }

        private BitmapImage _Thumbnail;
        //Implementation as String for beeing able to display thumbs
        public BitmapImage Thumbnail
        {
            get { return _Thumbnail;
            }

            set { this.RaiseAndSetIfChanged(x => x.Thumbnail, ref _Thumbnail, value); }
        }       

        public MultimediaObjectVM( MultimediaObject model)
        {
            Model = model;
            thumbnails.AsyncGet(Model).BindTo(this, x => x.Thumbnail);
        }        
    }
}
