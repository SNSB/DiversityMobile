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
using DiversityPhone.Model;
using Funq;
using System.IO.IsolatedStorage;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive;

namespace DiversityPhone.Services.BackgroundTasks
{
    public class UploadMultimediaTask 
    {
        
        private IFieldDataService Storage;
        private IDiversityServiceClient Repository;
        private IMultiMediaClient MMOSink;
        private INotificationService Notifications;
        private MultimediaObject _MMO;

        public static IObservable<Unit> Start(Container ioc, MultimediaObject mmo)
        {
            var t = new UploadMultimediaTask(ioc, mmo);
            return Observable.Start(t.Run);
        }


        private UploadMultimediaTask(Container ioc, MultimediaObject mmo)
        {
            Storage = ioc.Resolve<IFieldDataService>();
            Repository = ioc.Resolve<IDiversityServiceClient>();
            MMOSink = ioc.Resolve<IMultiMediaClient>();
            Notifications = ioc.Resolve<INotificationService>();

            _MMO = mmo;
        }


        void Run()
        {            
            var progress = new BehaviorSubject<string>("");
            Notifications.showProgress(progress);
            progress.OnNext(DiversityResources.UploadMultimediaTask_State_Uploading);

            var sinkUri = MMOSink.UploadMultiMediaObjectRawData(_MMO).First();
            if (String.IsNullOrWhiteSpace(sinkUri))
                throw new Exception("No value returned");
            _MMO.DiversityCollectionUri = sinkUri;
            Storage.updateMMOUri(_MMO.Uri, sinkUri);
            var success = Repository.InsertMultimediaObject(_MMO).First();
            Storage.updateMMOSuccessfullUpload(_MMO.Uri, sinkUri, success);

            progress.OnCompleted();            
        }
    }
}
