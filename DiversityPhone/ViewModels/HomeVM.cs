namespace DiversityPhone.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using Svc = DiversityPhone.Service;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using DiversityPhone.Services;
    using DiversityPhone.Model;
    using DiversityPhone.Messages;

    public class HomeVM : ReactiveObject
    {
        private IList<IDisposable> _subscriptions;

        #region Services
        private IMessageBus _messenger;
        private IOfflineStorage _storage;
        private Svc.IDiversityService _repository;

        private IObservable<Svc.HierarchySection> _uploadAsync;
        #endregion

        #region Commands
        public ReactiveCommand Settings { get; private set; }
        public ReactiveCommand Add { get; private set; }
        public ReactiveCommand GetVocabulary { get; private set; }
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
        #endregion

        public HomeVM(IMessageBus messenger, IOfflineStorage storage, Svc.IDiversityService repo)
        {
            _messenger = messenger;
            _storage = storage;
            _repository = repo;

            updateSeriesList();

            registerUpload();

            _subscriptions = new List<IDisposable>()
            {
                (Settings = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<Page>(Page.Settings)),    
                
                (Add = new ReactiveCommand())
                    .Subscribe(_ => addSeries()),

                (GetVocabulary = new ReactiveCommand())
                    .Subscribe(_ => getVoc()),              

                _messenger.Listen<EventSeries>(MessageContracts.SAVE)
                    .Subscribe(es => saveSeries(es))
            };

        }

        private void registerUpload()
        {
            var uploadHierarchy = Observable.FromAsyncPattern<Svc.HierarchySection, Svc.HierarchySection>(_repository.BeginInsertHierarchy, _repository.EndInsertHierarchy);
            (Upload = new ReactiveAsyncCommand())
                    .Select(_ => getSections().ToObservable()).First()
                    .Select(section => Tuple.Create(section, uploadHierarchy(section).First()))
                    .ForEach(updateTuple => _storage.updateHierarchy(updateTuple.Item1, updateTuple.Item2));
        }

        private IEnumerable<Svc.HierarchySection> getSections()
        {
            var eventseries = _storage.getNewEventSeries();
            
            foreach (var series in eventseries)
            {
                yield return _storage.getNewHierarchyBelow(series);
            }
        }

        private void getVoc()
        {
            var vocFunc = Observable.FromAsyncPattern<IList<DiversityPhone.Service.Term>>(_repository.BeginGetStandardVocabulary, _repository.EndGetStandardVocabulary);

            vocFunc.Invoke().Subscribe(voc => _storage.addTerms(voc.Select(
                wcf => new DiversityPhone.Model.Term()
                {
                    Code = wcf.Code,
                    Description = wcf.Description,
                    DisplayText = wcf.DisplayText,
                    ParentCode = wcf.ParentCode,
                    SourceID = wcf.SourceID
                })
                ));

            var taxonFunc = Observable.FromAsyncPattern<string, IEnumerable<Svc.TaxonName>>(_repository.BeginDownloadTaxonList, _repository.EndDownloadTaxonList);

            taxonFunc.Invoke("").Subscribe(taxa => _storage.addTaxonNames(taxa.Select(
                t => new Model.TaxonName()
                {
                    URI = t.URI,
                    TaxonNameSinAuth = t.TaxonNameSinAuth,
                    TaxonNameCache = t.TaxonNameCache,
                    SpeciesEpithet = t.SpeciesEpithet,
                    InfraspecificEpithet = t.InfraspecificEpithet,
                    GenusOrSupragenic = t.GenusOrSupragenic
                })));

        }        

        private void updateSeriesList()
        {
            SeriesList = new VirtualizingReadonlyViewModelList<EventSeries, EventSeriesVM>(
                _storage.getAllEventSeries(),
                (model) => new EventSeriesVM(model, _messenger)
                );
        }

        private void saveSeries(EventSeries es)
        {
            _storage.addOrUpdateEventSeries(es);
            updateSeriesList();
        }

        private void addSeries()
        {
            _messenger.SendMessage<EventSeries>(
                new EventSeries(),
                MessageContracts.EDIT
                );
        }
    }
}