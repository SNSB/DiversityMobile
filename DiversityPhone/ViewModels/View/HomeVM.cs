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
    using DiversityPhone.PhoneMediaService;
    using System.IO.IsolatedStorage;
    using System.IO;


    public class HomeVM : PageViewModel
    {
        private IList<IDisposable> _subscriptions;

        #region Services        
        private IOfflineStorage _storage;
        private IDiversityServiceClient _repository;
        //private DiversityService.DiversityServiceClient _plainUploadClient;
        private DiversityPhone.MediaService4.MediaService4Client _msc;
        private IObservable<Svc.HierarchySection> _uploadAsync;
        #endregion

        #region Commands
        public ReactiveCommand Settings { get; private set; }
        public ReactiveCommand Add { get; private set; }
        public ReactiveCommand GetVocabulary { get; private set; }
        public ReactiveCommand Maps { get; private set; }
        public ReactiveCommand UploadMMO { get; private set; }
        public ReactiveCommand UploadPlain { get; private set; }
        public ReactiveAsyncCommand Upload { get; private set; }        
        #endregion

        #region Properties
        private ObservableAsPropertyHelper<IList<EventSeriesVM>> _SeriesList;
        public IList<EventSeriesVM> SeriesList
        {
            get
            {
                return _SeriesList.Value;
            }            
        }

        private EventSeriesVM _NoEventSeries;
        public EventSeriesVM NoEventSeries
        {
            get
            {
                if (_NoEventSeries == null)
                    _NoEventSeries = new EventSeriesVM(Messenger, EventSeries.NoEventSeries, Page.ViewES);

                return _NoEventSeries;
            }
        }
        #endregion

        public HomeVM(IMessageBus messenger, IOfflineStorage storage, IDiversityServiceClient repo)
            : base(messenger)
        {            
            _storage = storage;
            _repository = repo;
            

            //Initialize MultimediaTranfsfer
           
            _msc=new MediaService4.MediaService4Client();
            _msc.SubmitCompleted+=new EventHandler<MediaService4.SubmitCompletedEventArgs>(msc_SubmitCompleted);
            _SeriesList = StateObservable
                .Select(_ => updatedSeriesList())
                .ToProperty(this, x => x.SeriesList);

            //Initialize PlainUpload
            //_plainUploadClient = new Svc.DiversityServiceClient();
            //_plainUploadClient.InsertEventSeriesCompleted+=new EventHandler<Svc.InsertEventSeriesCompletedEventArgs>(_plainUploadClient_InsertEventSeriesCompleted);
            //_plainUploadClient.InsertHierarchyCompleted += new EventHandler<Svc.InsertHierarchyCompletedEventArgs>(_plainUploadClient_InsertHierarchyCompleted);

            registerUpload();

            _subscriptions = new List<IDisposable>()
            {
                (Settings = new ReactiveCommand())
                    .Subscribe(_ => Messenger.SendMessage<Page>(Page.Settings)),    
                
                (Add = new ReactiveCommand())
                    .Subscribe(_ => addSeries()),
                (UploadMMO = new ReactiveCommand())
                    .Subscribe(_ => uploadMMos()),
                (GetVocabulary = new ReactiveCommand())
                    .Subscribe(_ => getVoc()), 
                (UploadPlain=new ReactiveCommand())
                    .Subscribe(_ =>uploadPlain()),
                (Maps=new ReactiveCommand())
                    .Subscribe(_ =>loadMapPage()),                
            };



        }

        private void registerUpload()
        {
            
            //var uploadHierarchy = Observable.FromAsyncPattern<Svc.HierarchySection, Svc.HierarchySection>(_repository.BeginInsertHierarchy, _repository.EndInsertHierarchy);
            //Upload = new ReactiveAsyncCommand();
            //    .Select(_ => getUploadSectionsForSeries().ToObservable()).First()
            //.Select(section => Tuple.Create(section, uploadHierarchy(section).First()))
            //.ForEach(updateTuple => _storage.updateHierarchy(updateTuple.Item1, updateTuple.Item2));
        }

        private IEnumerable<Svc.HierarchySection> getUploadSectionsForSeries(EventSeries es)
        {
            var events = _storage.getEventsForSeries(es)
                        .Where(ev => ev.ModificationState == null); // Only New Events
            
            foreach (var ev in events)
            {
                yield return _storage.getNewHierarchyToSyncBelow(ev);
            }
        }

        private void getVoc()
        {
            //var vocFunc = Observable.FromAsyncPattern<IList<DiversityPhone.DiversityService.Term>>(_repository.BeginGetStandardVocabulary, _repository.EndGetStandardVocabulary);

            //vocFunc.Invoke().Subscribe(voc => _storage.addTerms(voc.Select(
            //    wcf => new DiversityPhone.Model.Term()
            //    {
            //        Code = wcf.Code,
            //        Description = wcf.Description,
            //        DisplayText = wcf.DisplayText,
            //        ParentCode = wcf.ParentCode,
            //        SourceID = wcf.Source
            //    })
            //    ));


            //var taxonFunc = Observable.FromAsyncPattern<Svc.TaxonList,int, IEnumerable<Svc.TaxonName>>(_repository.BeginDownloadTaxonList, _repository.EndDownloadTaxonList);
            //var sampleTaxonList = new Svc.TaxonList() 
            //{ 
            //    Table = "TaxRef_BfN_VPlants",
            //    TaxonomicGroup = "plant",
            //    DisplayText = "Plants"
            //};
            
            ////TODO Page
            //taxonFunc.Invoke(sampleTaxonList,1).Subscribe(taxa => _storage.addTaxonNames(taxa.Select(
            //    t => new Model.TaxonName()
            //    {
            //        URI = t.URI,
            //        TaxonNameSinAuth = t.TaxonNameSinAuth,
            //        TaxonNameCache = t.TaxonNameCache,
            //        SpeciesEpithet = t.SpeciesEpithet,
            //        InfraspecificEpithet = t.InfraspecificEpithet,
            //        GenusOrSupragenic = t.GenusOrSupragenic
            //    }), sampleTaxonList));
            
        }

        private void uploadPlain()
        {

            IList<Svc.EventSeries> series = _storage.getUploadServiceEventSeries();
            if (series != null && series.Count > 0)
            {
                System.Collections.ObjectModel.ObservableCollection<Svc.EventSeries> es = GlobalUtility.ObservableConverter.ToObservableCollection<Svc.EventSeries>(series);
                _repository.InsertEventSeries(es);
            }
            else
            {
                syncHierarchies();
            }
        }

        private void syncHierarchies()
        {
            IList<EventSeries> seriesList = _storage.getAllEventSeries();
            foreach (EventSeries es in seriesList)
            {
                IList<Event> eventList = _storage.getEventsForSeries(es);
                foreach (Event ev in eventList)
                {
                    Svc.HierarchySection section = _storage.getNewHierarchyToSyncBelow(ev);
                    _repository.InsertHierarchy(section);
                }
            }
        }

        private void _plainUploadClient_InsertEventSeriesCompleted(object sender, DiversityService.InsertEventSeriesCompletedEventArgs args)
        {
            Dictionary<Svc.EventSeries, Svc.EventSeries> series = args.Result;
            foreach (KeyValuePair<Svc.EventSeries, Svc.EventSeries> kvp in series)
            {
                _storage.updateSeriesKey(kvp.Key.SeriesID, kvp.Value.SeriesID);
            }
            syncHierarchies();
        }


        private void _plainUploadClient_InsertHierarchyCompleted(object sender, DiversityService.InsertHierarchyCompletedEventArgs args)
        {
            Svc.KeyProjection keysToUpdate = args.Result;
            if (keysToUpdate.eventKey.Key != null && keysToUpdate.eventKey.Value!=null)
                _storage.updateEventKey((int) keysToUpdate.eventKey.Key, (int) keysToUpdate.eventKey.Value);
            foreach (KeyValuePair<int, int> specPair in keysToUpdate.specimenKeys)
                _storage.updateSpecimenKey(specPair.Key, specPair.Value);
            foreach (KeyValuePair<int, int> iuPair in keysToUpdate.iuKeys)
                _storage.updateIUKey(iuPair.Key, iuPair.Value);
        }




        #region Upload MMO
        private void uploadMMos()
        {
            //Testfunktion zur Übertagung eines MMO´s
            IList<MultimediaObject> mmoList = _storage.getAllMultimediaObjects();
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
                _msc.SubmitAsync(mmo.Uri, mmo.Uri, mmo.MediaType.ToString(),  0, 0, 0, "Test", DateTime.Now.ToShortDateString(), 371,data);
            }
        }

        private void msc_SubmitCompleted(object sender, MediaService4.SubmitCompletedEventArgs e)
        {
            String s = e.Result;
        }

        #endregion
        private IList<EventSeriesVM> updatedSeriesList()
        {
            return new VirtualizingReadonlyViewModelList<EventSeries, EventSeriesVM>(
                _storage.getAllEventSeries(),
                (model) => new EventSeriesVM(Messenger,model, Page.ViewES)
                );
        }        

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