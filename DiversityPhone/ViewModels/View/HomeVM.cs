﻿namespace DiversityPhone.ViewModels
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

    public class HomeVM : PageViewModel
    {
        private IList<IDisposable> _subscriptions;

        #region Services        
        private IOfflineStorage _storage;
        private Svc.IDiversityService _repository;

        private IObservable<Svc.HierarchySection> _uploadAsync;
        #endregion

        #region Commands
        public ReactiveCommand Settings { get; private set; }
        public ReactiveCommand Add { get; private set; }
        public ReactiveCommand GetVocabulary { get; private set; }
        public ReactiveCommand Maps { get; private set; }
        public ReactiveAsyncCommand Upload { get; private set; }        
        #endregion

        #region Properties
        private IList<EventSeriesVM> _SeriesList;
        public IList<EventSeriesVM> SeriesList
        {
            get
            {
                return _SeriesList;
            }
            private set
            {
                this.RaiseAndSetIfChanged(x => x.SeriesList, ref _SeriesList, value);
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

        public HomeVM(IMessageBus messenger, IOfflineStorage storage, Svc.IDiversityService repo)
            : base(messenger)
        {            
            _storage = storage;
            _repository = repo;

            updateSeriesList();

            registerUpload();

            _subscriptions = new List<IDisposable>()
            {
                (Settings = new ReactiveCommand())
                    .Subscribe(_ => Messenger.SendMessage<Page>(Page.Settings)),    
                
                (Add = new ReactiveCommand())
                    .Subscribe(_ => addSeries()),

                (GetVocabulary = new ReactiveCommand())
                    .Subscribe(_ => getVoc()),         
                (Maps=new ReactiveCommand())
                    .Subscribe(_ =>loadMapPage()),                
            };

        }

        private void registerUpload()
        {
            var uploadHierarchy = Observable.FromAsyncPattern<Svc.HierarchySection, Svc.HierarchySection>(_repository.BeginInsertHierarchy, _repository.EndInsertHierarchy);
            Upload = new ReactiveAsyncCommand();
                    /*.Select(_ => getUploadSectionsForSeries().ToObservable()).First()
                    .Select(section => Tuple.Create(section, uploadHierarchy(section).First()))
                    .ForEach(updateTuple => _storage.updateHierarchy(updateTuple.Item1, updateTuple.Item2));*/
        }

        private IEnumerable<Svc.HierarchySection> getUploadSectionsForSeries( EventSeries es)
        {
            var events = _storage.getEventsForSeries(es)
                        .Where(ev => ev.ModificationState == null); // Only New Events
            
            foreach (var series in events)
            {
                yield return _storage.getNewHierarchyBelow(series);
            }
        }

        private void getVoc()
        {
            var vocFunc = Observable.FromAsyncPattern<IList<DiversityPhone.DiversityService.Term>>(_repository.BeginGetStandardVocabulary, _repository.EndGetStandardVocabulary);

            vocFunc.Invoke().Subscribe(voc => _storage.addTerms(voc.Select(
                wcf => new DiversityPhone.Model.Term()
                {
                    Code = wcf.Code,
                    Description = wcf.Description,
                    DisplayText = wcf.DisplayText,
                    ParentCode = wcf.ParentCode,
                    SourceID = wcf.Source
                })
                ));

            var taxonFunc = Observable.FromAsyncPattern<Svc.TaxonList, IEnumerable<Svc.TaxonName>>(_repository.BeginDownloadTaxonList, _repository.EndDownloadTaxonList);
            var sampleTaxonList = new Svc.TaxonList() { Table = "TaxRef_BfN_VPlants" };
            taxonFunc.Invoke(sampleTaxonList).Subscribe(taxa => _storage.addTaxonNames(taxa.Select(
                t => new Model.TaxonName()
                {
                    URI = t.URI,
                    TaxonNameSinAuth = t.TaxonNameSinAuth,
                    TaxonNameCache = t.TaxonNameCache,
                    SpeciesEpithet = t.SpeciesEpithet,
                    InfraspecificEpithet = t.InfraspecificEpithet,
                    GenusOrSupragenic = t.GenusOrSupragenic
                }), sampleTaxonList));
            
        }

        

        private void updateSeriesList()
        {
            SeriesList = new VirtualizingReadonlyViewModelList<EventSeries, EventSeriesVM>(
                _storage.getAllEventSeries(),
                (model) => new EventSeriesVM(Messenger,model, Page.ViewES)
                );
        }

        private void saveSeries(EventSeries es)
        {
            _storage.addOrUpdateEventSeries(es);
            updateSeriesList();
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