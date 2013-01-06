
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System;


namespace DiversityPhone.ViewModels
{
    public class MultimediaObjectVM : ReactiveObject, IElementVM<MultimediaObject>
    {
        private static MemoryStream load_thumb(MultimediaObject mmo)
        {
            switch (mmo.MediaType)
            {
                case MediaType.Image:
                    if (!string.IsNullOrEmpty(mmo.Uri))
                    {
                        using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            if (isf.FileExists(mmo.Uri))
                            {
                                using (IsolatedStorageFileStream isfs = isf.OpenFile(mmo.Uri, FileMode.Open, FileAccess.Read))
                                {
                                    if (isfs.Length > int.MaxValue)
                                        throw new ArgumentException("file too big");

                                    var memory = new MemoryStream((int)isfs.Length);
                                    isfs.CopyTo(memory);
                                    return memory;
                                }
                            }
                        }
                    }
                    break;
                case MediaType.Audio:
                    break;
                case MediaType.Video:
                    break;
                default:
                    break;
            }
                
            
            
            return null;
        }
        private static ObservableAsyncMRUCache<MultimediaObject, MemoryStream> thumbnails = new ObservableAsyncMRUCache<MultimediaObject, MemoryStream>(mmo => Observable.Start(() => load_thumb(mmo)), 10);
        

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
            get { return _Thumbnail;  }

            set { this.RaiseAndSetIfChanged(x => x.Thumbnail, ref _Thumbnail, value); }
        }

        object IElementVM.Model
        {
            get { return Model; }
        }

        public MultimediaObjectVM( MultimediaObject model)
        {
            Model = model;

            Model.ObservableForProperty(x => x.Uri)                
                .Select(_ => Model)
                .StartWith(Model)                
                .SelectMany(mmo => thumbnails.AsyncGet(mmo))
                .Where(thumb => thumb != null)
                .ObserveOnDispatcher()
                .Select(thumb => { var img = new BitmapImage(); img.SetSource(thumb); return img; })                
                .Subscribe(img => Thumbnail = img);
        }        
    }
}
