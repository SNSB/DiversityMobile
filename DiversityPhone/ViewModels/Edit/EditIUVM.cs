using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DiversityPhone.ViewModels
{
    public class EditIUVM : EditPageVMBase<IdentificationUnit>
    {
        readonly ITaxonService Taxa;
        readonly IVocabularyService Vocabulary;
        readonly ILocationService Geolocation;
        readonly IFieldDataService Storage;

        private ReactiveAsyncCommand UpdateIdentifications = new ReactiveAsyncCommand();

        BehaviorSubject<Coordinate> _latest_location = new BehaviorSubject<Coordinate>(Coordinate.Unknown);
        IDisposable _location_subscription = Disposable.Empty;

        #region Properties
        private ObservableAsPropertyHelper<bool> _IsObservation;
        public bool IsObservation
        {
            get
            {
                return _IsObservation.Value;
            }
        }


        private bool _OnlyObserved;
        public bool OnlyObserved
        {
            get
            {
                return _OnlyObserved;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.OnlyObserved, ref _OnlyObserved, value);
            }
        }

        public ListSelectionHelper<Term> TaxonomicGroup { get; private set; }

        public ListSelectionHelper<Term> RelationshipType { get; private set; }

        private string _Description;
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Description, ref _Description, value);
            }
        }

        private string _QueryString;
        public string QueryString
        {
            get
            {
                return _QueryString;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.QueryString, ref _QueryString, value);
            }
        }

        public ListSelectionHelper<TaxonName> Identification { get; private set; }

        public ListSelectionHelper<Qualification> Qualifications { get; private set; }

        public bool IsToplevel { get { return _IsToplevel.Value; } }
        private ObservableAsPropertyHelper<bool> _IsToplevel;

        private DateTime _AnalysisDate;

        public DateTime AnalysisDate
        {
            get { return _AnalysisDate; }
            set { this.RaiseAndSetIfChanged(x => x.AnalysisDate, ref _AnalysisDate, value); }
        }


        #endregion



        public EditIUVM(
            ITaxonService Taxa,
            IVocabularyService Vocabulary,
            ILocationService Geolocation,
            IFieldDataService Storage,
            [Dispatcher] IScheduler Dispatcher,
            [ThreadPool] IScheduler ThreadPool
            )
        {
            this.Storage = Storage;
            this.Taxa = Taxa;
            this.Vocabulary = Vocabulary;
            this.Geolocation = Geolocation;

            TaxonomicGroup = new ListSelectionHelper<Term>(Dispatcher);
            RelationshipType = new ListSelectionHelper<Term>(Dispatcher);
            Identification = new ListSelectionHelper<TaxonName>(Dispatcher);
            Qualifications = new ListSelectionHelper<Model.Qualification>(Dispatcher);

            Observable.CombineLatest(
                CurrentModelObservable.Where(m => m.IsNew()),
                ActivationObservable,
                (_, act) => act
            )
                .Subscribe(active =>
                {
                    if (active)
                    {
                        _latest_location.OnNext(Coordinate.Unknown);
                        _location_subscription = Geolocation.Location().Where(l => !l.IsUnknown()).Subscribe(_latest_location);
                    }
                    else
                    {
                        _location_subscription.Dispose();
                    }
                });

            #region Update View
            _IsToplevel = this.ObservableToProperty(
                CurrentModelObservable
                .Select(m => m.RelatedUnitID == null),
                x => x.IsToplevel);

            var isObservation =
                CurrentModelObservable
                .Select(m => Storage.getSpecimenByID(m.SpecimenID))
                .Where(spec => spec != null)
                .Select(spec => spec.IsObservation());

            isObservation
                .CombineLatest(
                    ModelByVisitObservable
                    .Select(m => m.OnlyObserved)
                    .StartWith(false),
                    (isobs, onlyobs) => isobs || onlyobs)
                .Subscribe(x => OnlyObserved = x);

            _IsObservation = this.ObservableToProperty(
                isObservation, vm => vm.IsObservation);

            ModelByVisitObservable
                .Select(m => m.WorkingName)
                .Subscribe(x => QueryString = x ?? string.Empty);

            ModelByVisitObservable
                .Select(iu => iu.AnalysisDate)
                .Subscribe(date => AnalysisDate = date);
            #endregion

            #region Vocabulary

            var identificationQuery =
                this.WhenAny(x => x.QueryString, x => x.Value)
                    .CombineLatest(TaxonomicGroup.SelectedItemObservable, (query, tg) => System.Tuple.Create(query, tg))
                    .Publish();

            identificationQuery
                .Throttle(TimeSpan.FromMilliseconds(500), Dispatcher)
                .Subscribe(UpdateIdentifications.Execute);

            identificationQuery.Connect();


            var noUpdatesInFlight = UpdateIdentifications.ItemsInflight
                .Select(count => count == 0)
                .StartWith(true)
                .Replay(1, Dispatcher);

            noUpdatesInFlight.Connect();

            var canSave = Observable.CombineLatest(
                              (from tg in TaxonomicGroup.SelectedItemObservable
                               select tg != null),
                              (from id in Identification.SelectedItemObservable
                               select id != null && !string.IsNullOrWhiteSpace(id.TaxonNameCache)),
                              (from q in identificationQuery
                               select noUpdatesInFlight.StartWith(false))
                              .Switch(),
                              (a, b, c) => a && b && c
                                  
                              );

            canSave
                .DistinctUntilChanged()
                .Select(can_save =>
                    {
                        //immediately disable saving, delay reenabling it
                        if (can_save)
                            return Observable.Return(true).Delay(TimeSpan.FromMilliseconds(500), Dispatcher);
                        else
                            return Observable.Return(false);
                    })
                .Switch()
                .StartWith(false)
                .Subscribe(CanSaveSubject.OnNext);

            UpdateIdentifications
                .RegisterAsyncFunction(t_obj =>
                    {
                        var t = (System.Tuple<string, Term>)t_obj;
                        var candidates = Taxa.getTaxonNames(t.Item2, t.Item1).Take(10)
                        .SelectMany(tn =>
                            {
                                if (!string.IsNullOrWhiteSpace(tn.AcceptedNameURI))
                                {
                                    return new TaxonName[]
                                    {
                                        tn,
                                        new TaxonName() //Doesn't contain structured Information on Genus,...
                                        {
                                            TaxonNameCache = "= " + tn.AcceptedNameCache,
                                            URI = tn.AcceptedNameURI,
                                            Synonymy = DiversityPhone.Model.Synonymy.Accepted
                                        }
                                    };
                                }
                                else
                                    return new TaxonName[] { tn };
                            })
                        .ToList();

                        if (!string.IsNullOrWhiteSpace(t.Item1))
                        {
                            //Prepend WorkingName as Identification
                            candidates.Insert(0,
                                new TaxonName()
                                {
                                    TaxonNameCache = QueryString,
                                    GenusOrSupragenic = null,
                                    SpeciesEpithet = null,
                                    Synonymy = DiversityPhone.Model.Synonymy.WorkingName,
                                    URI = null,
                                    AcceptedNameCache = null,
                                    AcceptedNameURI = null
                                });
                        }
                        return candidates as IList<TaxonName>;
                    }, ThreadPool)
            .ObserveOn(Dispatcher)
            .Subscribe(Identification.ItemsObserver);

            ActivationObservable
                .Take(1)
                .Select(_ => Vocabulary.getTerms(TermList.TaxonomicGroups).ToList() as IList<Term>)
                .Subscribe(TaxonomicGroup.ItemsObserver);

            this.FirstActivation()
                .Select(_ => Vocabulary.getQualifications().ToList() as IList<Qualification>)
                .Subscribe(Qualifications.ItemsObserver);

            _IsToplevel
                .Where(isToplevel => !isToplevel)
                .Select(isToplevel => Vocabulary.getTerms(TermList.RelationshipTypes).ToList() as IList<Term>)
                .Subscribe(RelationshipType.ItemsObserver);
            #endregion

            #region Preserve Selections

            Identification.ItemsObservable
                .Where(x => x != null)
                .CombineLatest(ModelByVisitObservable.Where(m => m.IdentificationUri != null),
                (ids, model) => ids.FirstOrDefault(id => id.URI == Current.Model.IdentificationUri) ?? ids.FirstOrDefault())
                .Subscribe(x => Identification.SelectedItem = x);


            TaxonomicGroup.ItemsObservable
                .Where(x => x != null)
                .CombineLatest(ModelByVisitObservable.Where(m => m.TaxonomicGroup != null),
                (tgs, m) => tgs.FirstOrDefault(tg => tg.Code == Current.Model.TaxonomicGroup))
                .Subscribe(x => TaxonomicGroup.SelectedItem = x);


            RelationshipType.ItemsObservable
                .Where(x => x != null)
                .CombineLatest(ModelByVisitObservable.Where(m => m.RelationType != null),
                (rels, m) => rels.FirstOrDefault(rel => rel.Code == Current.Model.RelationType))
                .Where(x => x != null)
                .Subscribe(x => RelationshipType.SelectedItem = x);

            Qualifications.ItemsObservable
                .Where(x => x != null)
                .CombineLatest(ModelByVisitObservable,
                (qualis, m) => qualis.FirstOrDefault(q => q.Code == m.Qualification))
                .Where(x => x != null)
                .Subscribe(x => Qualifications.SelectedItem = x);
            #endregion

            var saveTaxonGroupSelection = Save
                .Select(_ => TaxonomicGroup.SelectedItem);
            Messenger.RegisterMessageSource(
                saveTaxonGroupSelection,
                MessageContracts.USE);

            saveTaxonGroupSelection
                .Select(g => bringItemToTop(TaxonomicGroup.Items, g))
                .Subscribe(TaxonomicGroup.ItemsObserver);

            Messenger.RegisterMessageSource(
                Save
                .Select(_ => RelationshipType.SelectedItem),
                MessageContracts.USE);
        }

        private IList<T> bringItemToTop<T>(IEnumerable<T> oldlist, T item) where T : class
        {
            IList<T> res = new List<T>();
            res.Add(item);
            foreach (var other in oldlist.Where(x => x != item))
            {
                res.Add(other);
            }
            return res;
        }



        protected override void UpdateModel()
        {
            if (!Current.Model.IsLocalized())
                Current.Model.SetCoordinates(_latest_location.FirstOrDefaultAsync().Wait() ?? Coordinate.Unknown);
            Current.Model.TaxonomicGroup = TaxonomicGroup.SelectedItem.Code;
            Current.Model.WorkingName = Identification.SelectedItem.TaxonNameCache.TrimStart(new[] { ' ', '=' });
            Current.Model.OnlyObserved = this.OnlyObserved;
            Current.Model.IdentificationUri = Identification.SelectedItem.URI;
            Current.Model.RelationType = (RelationshipType.SelectedItem != null) ? RelationshipType.SelectedItem.Code : null;
            Current.Model.Qualification = (Qualifications.SelectedItem != null) ? Qualifications.SelectedItem.Code : null;
            Current.Model.AnalysisDate = AnalysisDate;
        }
    }
}
