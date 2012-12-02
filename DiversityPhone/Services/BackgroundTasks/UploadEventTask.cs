using System;
using DiversityPhone.Model;
using Funq;
using System.Reactive.Linq;
using System.Linq;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using System.Reactive.Subjects;
using System.Reactive;


namespace DiversityPhone.Services.BackgroundTasks
{
    public class UploadEventTask 
    {     

        IDiversityServiceClient Repo;
        IFieldDataService Storage;
        INotificationService Notifications;
        Event _Event;

        public static IObservable<Unit> Start(Container ioc, Event ev)
        {
            var t = new UploadEventTask(ioc, ev);
            return Observable.Start(t.Run); 

        }



        private UploadEventTask(Container ioc, Event ev)
        {
            Repo = ioc.Resolve<IDiversityServiceClient>();
            Storage = ioc.Resolve<IFieldDataService>();
            Notifications = ioc.Resolve<INotificationService>();
            if (ev == null)
                throw new ArgumentNullException("ev");

            _Event = ev;

        }

        protected void Run()
        {   
            var progress = new BehaviorSubject<string>("");
            Notifications.showProgress(progress);

            progress.OnNext(DiversityResources.UploadEventTask_State_UploadingSeries);
            var series = Storage.getEventSeriesByID(_Event.SeriesID);
            if (series.IsModified())
            {
                var collectionKey = Repo.InsertEventSeries(series).First();
                series.DiversityCollectionEventSeriesID = collectionKey;
                Storage.updateSeriesKey(series.SeriesID.Value, collectionKey);
            }

            progress.OnNext(DiversityResources.UploadEventTask_State_UploadingEvent);
            if (!_Event.DiversityCollectionSeriesID.HasValue)
                _Event.DiversityCollectionSeriesID = series.DiversityCollectionEventSeriesID;

            var hierarchy = Storage.getNewHierarchyToSyncBelow(_Event);
            var collectionKeys = Repo.InsertHierarchy(hierarchy).First();
                
            if (collectionKeys.eventKey.Any())
            {
                var evKeyProjection = collectionKeys.eventKey.First();

                Storage.updateEventKey(evKeyProjection.Key, evKeyProjection.Value);
            }

            if (collectionKeys.specimenKeys.Any())
            {
                foreach (var specKey in collectionKeys.specimenKeys)
                {
                    Storage.updateSpecimenKey(specKey.Key, specKey.Value);
                }
            }

            if (collectionKeys.iuKeys.Any())
            {
                foreach (var iuKey in collectionKeys.iuKeys)
                {
                    Storage.updateIUKey(iuKey.Key, iuKey.Value);
                }
            }

            progress.OnCompleted();
            
        }
    }
}
