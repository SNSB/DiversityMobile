using DiversityPhone.Interface;
using DiversityPhone.Model;
using DiversityPhone.Services;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace DiversityPhone.ViewModels
{
    public class VideoVM : EditPageVMBase<MultimediaObject>
    {
        private readonly IStoreMultimedia VideoStore;

        private IVideoService _VideoService;

        public IVideoService VideoService
        {
            get { return _VideoService; }
            set { this.RaiseAndSetIfChanged(x => x.VideoService, ref _VideoService, value); }
        }

        public VideoVM(IStoreMultimedia VideoStore)
            : base(mmo => mmo.MediaType == MediaType.Video)
        {
            this.VideoStore = VideoStore;

            ModelByVisitObservable
                .Where(_ => VideoService != null)
                .Subscribe(m =>
                {
                    if (m.IsNew())
                    {
                        VideoService.CreateRecording();
                    }
                    else
                    {
                        var videoFile = VideoStore.GetMultimedia(m.Uri);
                        VideoService.SetVideoFile(videoFile);
                    }
                }
                );

            CanSave().Subscribe(CanSaveSubject);
        }

        protected override async Task UpdateModel()
        {
            var video = VideoService;
            if (video != null)
            {
                var fileStream = video.GetRecording();
                Current.Model.Uri = VideoStore.StoreMultimedia(Current.Model.NewFileName(), fileStream);
            }
        }

        protected IObservable<bool> CanSave()
        {
            return this.WhenAny(x => x.VideoService, x => x.GetValue())
                .Select(x => (x != null) ? x.HasRecording() : Observable.Empty<bool>())
                .Switch();
        }
    }
}