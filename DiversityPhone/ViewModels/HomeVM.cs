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
        private INavigationService _navigation;
        private IOfflineStorage _storage;
        private Svc.IDiversityService _repository;
        #endregion

        #region Commands
        public ReactiveCommand Settings { get; private set; }
        public ReactiveCommand Add { get; private set; }
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

        public HomeVM(IMessageBus messenger, INavigationService nav, IOfflineStorage storage, Svc.IDiversityService repo)
        {
            _messenger = messenger;
            _navigation = nav;
            _storage = storage;
            _repository = repo;

            updateSeriesList();
            _messenger.Listen<EventSeries>(MessageContracts.SAVE)
                .Subscribe(es => saveSeries(es));

            _messenger.Listen<EventSeries>(MessageContracts.SELECT)
                .Subscribe(es => selectSeries(es));

            _subscriptions = new List<IDisposable>()
            {
                (Settings = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<Page>(Page.Settings)),    
                
                  (Add = new ReactiveCommand())
                    .Subscribe(_ => addSeries()),

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

        private void selectSeries(EventSeries es)
        {
            _navigation.Navigate(Page.ViewES);
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
            _storage.addEventSeries(es);
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