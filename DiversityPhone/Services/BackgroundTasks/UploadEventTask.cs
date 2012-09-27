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
using System.Reactive.Linq;
using System.Linq;
using Svc = DiversityPhone.DiversityService;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using Newtonsoft.Json;


namespace DiversityPhone.Services.BackgroundTasks
{
    public class UploadEventTask : BackgroundTask
    {
        private const string UNIT_KEY = "S";
        private const string PROJECTION_KEY = "P";

        IDiversityServiceClient Repo;
        IFieldDataService Storage;

        public UploadEventTask(Container ioc)
        {
            Repo = ioc.Resolve<IDiversityServiceClient>();
            Storage = ioc.Resolve<IFieldDataService>();
        }


        public override bool CanResume
        {
            get { return true; }
        }

        protected override void saveArgumentToState(object arg)
        {
            var ev = arg as DiversityPhone.Model.Event;
            if (ev != null)
            {
                
                State[UNIT_KEY] = ev.EventID.ToString();
            }
        }

        protected override object getArgumentFromState()
        {            
            if (State.ContainsKey(UNIT_KEY))
            {
                return Storage.getEventByID(int.Parse(State[UNIT_KEY]));          
            }

            return null;
        }

        protected override void Run(object arg)
        {
            var unit = arg as Event;
            if (unit != null)
            {
                var series = Storage.getEventSeriesByID(unit.SeriesID);
                if (series.IsModified())
                {
                    var collectionKey = Repo.InsertEventSeries(series).First();
                    series.DiversityCollectionEventSeriesID = collectionKey;
                    Storage.updateSeriesKey(series.SeriesID.Value, collectionKey);
                }

                var collectionKeys = loadKeys();
                if (collectionKeys == null)
                {
                    var ev = unit; 
                    if (!ev.DiversityCollectionSeriesID.HasValue)
                        ev.DiversityCollectionSeriesID = series.DiversityCollectionEventSeriesID;

                    var hierarchy = Storage.getNewHierarchyToSyncBelow(ev);
                    collectionKeys = Repo.InsertHierarchy(hierarchy).First();
                    saveKeys(collectionKeys);
                }
                
                
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
            }
        }

        void saveKeys(DiversityPhone.DiversityService.KeyProjection keys)
        {
            State[PROJECTION_KEY] = JsonConvert.SerializeObject(keys);
        }

        DiversityPhone.DiversityService.KeyProjection loadKeys()
        {
            try
            {
               
                if (State.ContainsKey(PROJECTION_KEY))
                {
                    return JsonConvert.DeserializeObject<DiversityPhone.DiversityService.KeyProjection>(State[PROJECTION_KEY]);
                }
                return null;
            }
            catch (Exception ex)
            {                
                throw ex ;
            }
        }


        protected override void Cancel()
        {
            
        }

        protected override void Cleanup(object arg)
        {
            
        }
    }
}
