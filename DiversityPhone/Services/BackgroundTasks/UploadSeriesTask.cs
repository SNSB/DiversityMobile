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

namespace DiversityPhone.Services.BackgroundTasks
{
    public class UploadSeriesTask : BackgroundTask
    {
        private const string UNIT_KEY = "S";        

        IDiversityServiceClient Repo;
        IFieldDataService Storage;

        public UploadSeriesTask(Container ioc)
        {
            Repo = ioc.Resolve<IDiversityServiceClient>();
            Storage = ioc.Resolve<IFieldDataService>();
        }


        public override bool CanResume
        {
            get { return false; }
        }

        protected override void saveArgumentToState(object arg)
        {
            var series = arg as SyncUnit;
            if (series != null)
            {
                var serializer = new XmlSerializer(typeof(SyncUnit));
                var ms = new MemoryStream();
                serializer.Serialize(ms,series);
                var reader = new StreamReader(ms);
                reader.BaseStream.Seek(0,SeekOrigin.Begin);
                State[UNIT_KEY] = reader.ReadToEnd();
            }
        }

        protected override object getArgumentFromState()
        {
            var serializer = new XmlSerializer(typeof(SyncUnit));
            if (State.ContainsKey(UNIT_KEY))
            {                
                var reader = XmlReader.Create(new StringReader(State[UNIT_KEY]));

                if (serializer.CanDeserialize(reader))
                    return serializer.Deserialize(reader);                
            }

            return null;
        }

        protected override void Run(object arg)
        {
            var unit = arg as SyncUnit;
            if (unit != null)
            {
                var series = Storage.getEventSeriesByID(unit.SeriesID);
                if (series.IsModified())
                {
                    var collectionKey = Repo.InsertEventSeries(series).First();
                    Storage.updateSeriesKey(series.SeriesID, collectionKey);
                }
                
                var ev = Storage.getEventByID(unit.EventID);
                if (!ev.DiversityCollectionSeriesID.HasValue)
                    ev.DiversityCollectionSeriesID = series.DiversityCollectionEventSeriesID;

                var hierarchy = Storage.getNewHierarchyToSyncBelow(ev);
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
