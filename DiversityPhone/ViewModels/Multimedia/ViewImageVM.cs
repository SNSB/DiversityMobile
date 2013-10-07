using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;

namespace DiversityPhone.ViewModels
{
    public class ViewImageVM : ViewPageVMBase<MultimediaObject>,  IDeletePageVM
    {
        readonly IStoreImages ImageStore;

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

        public IReactiveCommand Delete { get; private set; }


        public ViewImageVM(IStoreImages ImageStore, 
            [Dispatcher] IScheduler Dispatcher)
            : base( mmo => mmo.MediaType == MediaType.Image)
        {
            this.ImageStore = ImageStore;

            Messenger
                .Listen<IElementVM<MultimediaObject>>(MessageContracts.VIEW)
                .Where(vm => vm.Model.MediaType == MediaType.Image && !vm.Model.IsNew())
                .Subscribe(x => Current = x);

            
            //View Old image
            CurrentModelObservable
                .ObserveOn(Dispatcher)       
                .Where(mmo => !mmo.IsNew())
                .Select(mmo =>
                    {
                        var img = new BitmapImage();
                        try
                        {
                            var fileStream = ImageStore.GetMultimedia(mmo.Uri);
                            img.SetSource(fileStream);
                            return img;
                        }
                        catch(Exception)
                        {
                            return null;
                        }
                    })
                .Subscribe(img => CurrentImage = img);

            Delete = this.CurrentObservable.DeleteCommand(Messenger);
        }
    }
}
