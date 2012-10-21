using System;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using System.Reactive.Linq;
using DiversityPhone.Services;
using System.Collections.Generic;
using Funq;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Devices;
using System.Windows;
using Microsoft.Phone.Tasks;
using System.Reactive;
using System.Reactive.Concurrency;

namespace DiversityPhone.ViewModels
{
    public class ImageVM : EditPageVMBase<MultimediaObject>
    {
        private BitmapImage _CurrentImage;

        public BitmapImage CurrentImage
        {
            get
            {
                return _CurrentImage;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.CurrentImage, ref _CurrentImage, value);
            }
        }

        public ReactiveCommand Take { get; private set; } 
 
        


        public ImageVM()
            : base( mmo => mmo.MediaType == MediaType.Image)
        {
            Messenger
                .Listen<IElementVM<MultimediaObject>>(MessageContracts.VIEW)
                .Where(vm => vm.Model.MediaType == MediaType.Image)
                .BindTo(this, x => x.Current);

            Take = new ReactiveCommand();

            CanSave().Subscribe(CanSaveSubject);
            //New Image
            this.OnActivation()                
                .Where(_ => Current.Model.IsNew())
                .Merge(Take.Select(_ => Unit.Default))
                .Select(_ => new CameraCaptureTask())
                .SelectMany(capture =>
                    {
                        var results =
                            Observable.FromEventPattern<PhotoResult>(h => capture.Completed += h, h => capture.Completed -= h)
                            .Select(ev => ev.EventArgs)
                            .Take(1)
                            .Catch(Observable.Empty<PhotoResult>())
                            .Replay(1);
                        results.Connect();
                        try
                        {
                            capture.Show();
                        }
                        catch (InvalidOperationException)
                        {
                            return Observable.Empty<PhotoResult>();
                        }
                        return results;
                    })
                .Where(photo => photo.TaskResult == TaskResult.OK)
                .Select(res =>
                    {
                        var img = new BitmapImage();
                        img.SetSource(res.ChosenPhoto);
                        return img;
                    })
                .Merge(
                    //View Old image
                    CurrentModelObservable                        
                        .Where(mmo => !mmo.IsNew())
                        .Select(mmo =>
                            {
                                var img = new BitmapImage();
                                try
                                {
                                    using(var file = IsolatedStorageFile.GetUserStoreForApplication().OpenFile(mmo.Uri, FileMode.Open))
                                    {
                                        img.SetSource(file);
                                        return img;
                                    }
                                }
                                catch(Exception)
                                {
                                    return null;
                                }
                            })   
                 )
                .ObserveOnDispatcher()
                .Subscribe(img => CurrentImage = img);
        }

        protected override void UpdateModel()
        {
            var newURI = saveImageAndGetURI();
            var previousURI = Current.Model.Uri;

            Current.Model.Uri = newURI;

            deleteImageIfExists(previousURI);           
        }
       
        protected IObservable<bool> CanSave()
        {
            return this.ObservableForProperty(x => x.CurrentImage)
                .Select(image=> image!=null)
                .StartWith(false);
        }


        private string getImageUri()
        {                        
            //Create filename for JPEG in isolated storage as a Guid and filename for thumb
            return string.Format("{0}.jpg",Guid.NewGuid()); 
        }

        private string saveImageAndGetURI()
        {
            var newURI = getImageUri();
            using (IsolatedStorageFileStream myFileStream = IsolatedStorageFile.GetUserStoreForApplication().CreateFile(newURI))
            {
                //Encode the WriteableBitmap into JPEG stream and place into isolated storage.
                System.Windows.Media.Imaging.Extensions.SaveJpeg(new WriteableBitmap(this.CurrentImage), myFileStream, this.CurrentImage.PixelWidth, this.CurrentImage.PixelHeight, 0, 85);
            }

            return newURI;
        }

        private void deleteImageIfExists(string uri)
        {
            var previousURI = Current.Model.Uri;
            //Create virtual store and file stream. Check for duplicate tempJPEG files.
            var myStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (string.IsNullOrWhiteSpace(previousURI) && myStore.FileExists(previousURI))
            {
                myStore.DeleteFile(previousURI);
            }
        }
    }
}
