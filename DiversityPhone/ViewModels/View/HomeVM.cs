
namespace DiversityPhone.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using Svc = DiversityPhone.DiversityService;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using DiversityPhone.Services;
    using DiversityPhone.Model;
    using DiversityPhone.Messages;
    using System.IO.IsolatedStorage;
    using System.IO;
    using System.Collections.ObjectModel;
    using GlobalUtility;
    using System.Windows;


    public class HomeVM : PageViewModel
    {
        private IList<IDisposable> _subscriptions;

        int syncingHierarchies = 0;
        #region Services        
        private IFieldDataService _storage;
        private IDiversityServiceClient _repository;
        private ISettingsService _settings;
        private DiversityService.DiversityServiceClient _plainUploadClient;
        private DiversityPhone.MediaService4.MediaService4Client _msc;
        private IObservable<Svc.HierarchySection> _uploadAsync;
        private MultimediaObject actualMMOSync;
        #endregion

        #region Commands
        public ReactiveCommand Settings { get; private set; }
        public ReactiveCommand Add { get; private set; }       
        public ReactiveCommand Maps { get; private set; }
        public ReactiveCommand UploadMMO { get; private set; }
        public ReactiveCommand UploadPlain { get; private set; }
        public ReactiveAsyncCommand Upload { get; private set; }        
        #endregion

        #region Properties        
        public ReactiveCollection<EventSeriesVM> SeriesList
        {
            get;
            private set;
        }

        public EventSeriesVM NoEventSeries { get; private set; }
        #endregion

        public HomeVM(IMessageBus messenger, IFieldDataService storage, IDiversityServiceClient repo, ISettingsService settings)
            : base(messenger)
        {            
            _storage = storage;
            _repository = repo;
            _settings = settings;

            NoEventSeries = new EventSeriesVM(EventSeries.NoEventSeries);
            NoEventSeries
                .SelectObservable
                .Select(_ => (string)null)
                .ToNavigation(Page.ViewES);
            

            //Initialize MultimediaTransfer
           
            _msc=new MediaService4.MediaService4Client();
            _msc.SubmitCompleted+=new EventHandler<MediaService4.SubmitCompletedEventArgs>(msc_SubmitCompleted);

            
            var series = StateObservable
                .Select(_ => storage.getAllEventSeries())
                .Publish();
            series.Connect();

            SeriesList = 
                series
                .Do(_ => SeriesList.Clear())
                .SelectMany(list => list.Select(s => new EventSeriesVM(s)))
                .Do(vm => vm.SelectObservable
                    .Select(sender => sender.Model.SeriesID.ToString())
                    .ToNavigation(Page.ViewES)
                    )
                .CreateCollection();
                    

            //Initialize PlainUpload
            _plainUploadClient = new Svc.DiversityServiceClient();
            _plainUploadClient.InsertEventSeriesCompleted+=new EventHandler<Svc.InsertEventSeriesCompletedEventArgs>(_plainUploadClient_InsertEventSeriesCompleted);
            _plainUploadClient.InsertHierarchyCompleted += new EventHandler<Svc.InsertHierarchyCompletedEventArgs>(_plainUploadClient_InsertHierarchyCompleted);
            _plainUploadClient.InsertMMOCompleted+=new EventHandler<Svc.InsertMMOCompletedEventArgs>(_plainUploadClient_InsertMMOCompleted);

            var noOpenSeries = series
                .Select(list => list.Any(s => s.SeriesEnd == null))
                .Select(openseries => !openseries);

            _subscriptions = new List<IDisposable>()
            {
                (Settings = new ReactiveCommand())
                    .Subscribe(_ => Messenger.SendMessage<Page>(Page.Settings)),    
                
                (Add = new ReactiveCommand(noOpenSeries))
                    .Subscribe(_ => addSeries()),
                (UploadMMO = new ReactiveCommand())
                    .Subscribe(_ => uploadFirstMMo()),               
                (UploadPlain=new ReactiveCommand())
                    .Subscribe(_ =>uploadPlain()),
                (Maps=new ReactiveCommand())
                    .Subscribe(_ =>loadMapPage()),                
            };

            
        }

  
        private IEnumerable<Svc.HierarchySection> getUploadSectionsForSeries(EventSeries es)
        {
            var events = _storage.getEventsForSeries(es); // All Events because Specimen and Units may be added in existing events.
            
            foreach (var ev in events)
            {
                yield return _storage.getNewHierarchyToSyncBelow(ev);
            }
        }
       

        private void uploadPlain()
        {
            
            IList<EventSeries> series = _storage.getUploadServiceEventSeries();

            if (series != null && series.Count > 0)
            {
               IList<Svc.EventSeries> convertSeries = new List<Svc.EventSeries>();
               foreach (EventSeries es in series)
               {
                   Svc.EventSeries sES=EventSeries.ToServiceObject(es);
                   //sES.Geography = _storage.convertGeoPointsToString(es.SeriesID);
                   convertSeries.Add(sES);
               }
               ObservableCollection<Svc.EventSeries> seriesConv = ObservableConverter.ToObservableCollection<Svc.EventSeries>(convertSeries);
               _plainUploadClient.InsertEventSeriesAsync(seriesConv, _repository.GetCreds());
              
            }
            else
            {
                syncHierarchies();
            }
        }

        private void syncHierarchies()
        {
            IList<Event> eventList = _storage.getAllEvents();
            syncingHierarchies = eventList.Count;
            foreach (Event ev in eventList)
            {
                Svc.HierarchySection section = _storage.getNewHierarchyToSyncBelow(ev);
                Svc.UserCredentials cred = _repository.GetCreds();
                _plainUploadClient.InsertHierarchyAsync(section, cred);
            }  
        }

        private void _plainUploadClient_InsertEventSeriesCompleted(object sender, DiversityService.InsertEventSeriesCompletedEventArgs args)
        {
            try
            {
                Dictionary<int, int> series = args.Result;
                foreach (var kvp in series)
                {
                    _storage.updateSeriesKey(kvp.Key, kvp.Value);
                }
            } catch (Exception e)
            {
                MessageBox.Show("SyncError: " + e.Message);
            }
  
            MessageBox.Show("Syncing EventSeries Complete");
            syncHierarchies();
           
        }


        private void _plainUploadClient_InsertHierarchyCompleted(object sender, DiversityService.InsertHierarchyCompletedEventArgs args)
        {
           
            try
            {
                Svc.KeyProjection keysToUpdate = args.Result;
                foreach (KeyValuePair<int, int> evPair in keysToUpdate.eventKey)
                    _storage.updateEventKey(evPair.Key, evPair.Value);
                foreach (KeyValuePair<int, int> specPair in keysToUpdate.specimenKeys)
                    _storage.updateSpecimenKey(specPair.Key, specPair.Value);
                foreach (KeyValuePair<int, int> iuPair in keysToUpdate.iuKeys)
                    _storage.updateIUKey(iuPair.Key, iuPair.Value);
            }
            catch (Exception e)
            {
                MessageBox.Show("SyncError: " + e.Message);
            }
            syncingHierarchies--;
            if (syncingHierarchies == 0)
            {
                MessageBox.Show("Syncing Hierarchies Complete");
            }
        }




        #region Upload MMO
        private void uploadFirstMMo()
        {
            //Testfunktion zur Übertagung eines MMO´s
            IList<MultimediaObject> mmoList = _storage.getMultimediaObjectsForUpload();
            if (mmoList != null && mmoList.Count > 0)
            {
                MultimediaObject mmo = mmoList.First();

                byte[] data;
                // Read the entire image in one go into a byte array
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    // Open the file - error handling omitted for brevity
                    // Note: If the image does not exist in isolated storage the following exception will be generated:
                    // System.IO.IsolatedStorage.IsolatedStorageException was unhandled
                    // Message=Operation not permitted on IsolatedStorageFileStream

                    using (IsolatedStorageFileStream isfs = isf.OpenFile(mmo.Uri, FileMode.Open, FileAccess.Read))
                    {

                        // Allocate an array large enough for the entire file
                        data = new byte[isfs.Length];
                        // Read the entire file and then close it
                        isfs.Read(data, 0, data.Length);
                        isfs.Close();
                    }

                }
                this.actualMMOSync = mmo;
                _msc.SubmitAsync(mmo.Uri, mmo.Uri, mmo.MediaType.ToString(),  0, 0, 0, "Test", DateTime.Now.ToShortDateString(), 371,data);
            }
        }

        private void msc_SubmitCompleted(object sender, MediaService4.SubmitCompletedEventArgs e)
        {
            String s = e.Result;
            try
            {
                MultimediaObject mmo = actualMMOSync;//Alternativ über Guid bauen und aus storge ziehen
                mmo.DiversityCollectionUri = e.Result;
                _storage.updateMMOUri(mmo.Uri, mmo.DiversityCollectionUri);
                Svc.MultimediaObject serviceMmo = MultimediaObject.ToServiceObject(mmo);
                _plainUploadClient.InsertMMOAsync(serviceMmo, _repository.GetCreds());
            }
            catch (Exception)
            {
                //Todo Handling
            }
        }

        private void _plainUploadClient_InsertMMOCompleted(object sender, DiversityService.InsertMMOCompletedEventArgs args)
        {
            try
            {
                bool uploadsuccess = args.Result;
                if (uploadsuccess == true)
                {
                    _storage.updateMMOState(actualMMOSync.DiversityCollectionUri);
                }
                else
                {
                    //Not Succesfull handle here
                }
            }
            catch (Exception)
            {
                //Todo Handling
            }
        }

        #endregion
             

        private void addSeries()
        {
            Messenger.SendMessage<NavigationMessage>(new NavigationMessage(Page.EditES,null));
        }

        private void loadMapPage()
        {
            Messenger.SendMessage<NavigationMessage>(new NavigationMessage(Page.LoadedMaps, null));
        }


       
    }
}