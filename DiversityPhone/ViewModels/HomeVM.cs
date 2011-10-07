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

    public class HomeVM : ReactiveObject
    {
        private IList<IDisposable> _subscriptions;

        #region Services
        private IMessageBus _messenger;
        private IOfflineStorage _storage;
        private Svc.IDiversityService _repository;
        #endregion

        #region Commands
        public ReactiveCommand Settings { get; private set; }
        public ReactiveCommand AddSeries { get; private set; }
        public ReactiveCommand GetVocabulary { get; private set; }
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

            _subscriptions = new List<IDisposable>()
            {
                (Settings = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<Page>(Page.Settings)),    
                
                (AddSeries = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<Model.EventSeries>(new Model.EventSeries())), 

                (GetVocabulary = new ReactiveCommand())
                    .Subscribe(_ => getVoc()),
            };

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
                    TaxonomicGroup = "plant",
                    TaxonNameSinAuth = t.TaxonNameSinAuth,
                    TaxonNameCache = t.TaxonNameCache,
                    SpeciesEpithet = t.SpeciesEpithet,
                    InfraspecificEpithet = t.InfraspecificEpithet,
                    GenusOrSupragenic = t.GenusOrSupragenic
                })));

        }

        private void updateSeriesList()
        {
            SeriesList = new VirtualizingReadonlyViewModelList<Model.EventSeries, EventSeriesVM>(
                _storage.getAllEventSeries(),
                (model) => new EventSeriesVM(model, _messenger)
                );
        }
    }
}