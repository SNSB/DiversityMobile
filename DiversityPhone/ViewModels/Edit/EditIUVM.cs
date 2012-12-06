using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using Svc = DiversityPhone.DiversityService;
using ReactiveUI;
using ReactiveUI.Xaml;
using System.Reactive.Subjects;
using DiversityPhone.Model;
using Funq;
using System.Device.Location;
using System.Reactive.Disposables;

namespace DiversityPhone.ViewModels
{
    public class EditIUVM : EditPageVMBase<IdentificationUnit>
    {
        private ITaxonService Taxa;
        private IVocabularyService Vocabulary;
        private ILocationService Geolocation;
        private IFieldDataService Storage;

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

        public ListSelectionHelper<TaxonSynonymyPair> Identification { get; private set; }

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



        public EditIUVM(Container ioc)
        {
            Storage = ioc.Resolve<IFieldDataService>();
            Taxa = ioc.Resolve<ITaxonService>();
            Vocabulary = ioc.Resolve<IVocabularyService>();
            Geolocation = ioc.Resolve<ILocationService>();

            TaxonomicGroup = new ListSelectionHelper<Term>();
            RelationshipType = new ListSelectionHelper<Term>();
            Identification = new ListSelectionHelper<TaxonSynonymyPair>();
            Qualifications = new ListSelectionHelper<Model.Qualification>();

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

            registerCanSave();         

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
                .Where(wn => !string.IsNullOrWhiteSpace(wn))
                .Subscribe(x => QueryString = x);

            ModelByVisitObservable
                .Select(iu => iu.AnalysisDate)
                .Subscribe(date => AnalysisDate = date);
            #endregion

            #region Vocabulary
            
            this.ObservableForProperty(vm => vm.QueryString)
            .Value()
            .CombineLatest(TaxonomicGroup,
                (query, tg) =>
                {
                    IList<TaxonName> rawTaxa= Taxa.getTaxonNames(tg, query);
                    IList<TaxonSynonymyPair> refinendTaxa = new List<TaxonSynonymyPair>();
                    foreach (TaxonName tn in rawTaxa)
                    {
                        if (tn.Synonymy == Synonymy.Accepted)
                            refinendTaxa.Add(new TaxonSynonymyPair(tn));
                        else
                        {
                            if (tn.AcceptedNameURI != null && !tn.AcceptedNameURI.Equals(String.Empty))
                            {
                                TaxonName acceptedName = (new TaxonName() //Doesn´t contain structured Information on Genus,...
                                {
                                    TaxonNameCache = tn.AcceptedNameURI,
                                    URI = tn.AcceptedNameURI,
                                    Synonymy = DiversityPhone.Model.Synonymy.Accepted
                                });
                                refinendTaxa.Add(new TaxonSynonymyPair(acceptedName, tn));
                            }
                            else
                            {
                                TaxonSynonymyPair tsp = new TaxonSynonymyPair(null, tn);
                                tsp.Selected = tn;
                                refinendTaxa.Add (tsp);
                            }
                        }
                       
                    }
                    return refinendTaxa;
                })
            .Select( candidates =>
                {
                    if (!string.IsNullOrWhiteSpace(QueryString))
                    {
                        //Prepend WorkingName as Identification
                        candidates.Insert(0,
                            new TaxonSynonymyPair(
                            new TaxonName()
                            {
                                TaxonNameCache = QueryString,
                                GenusOrSupragenic = null,
                                SpeciesEpithet = null,
                                Synonymy = DiversityPhone.Model.Synonymy.WorkingName,
                                URI = null,
                                AcceptedNameCache=null,
                                AcceptedNameURI=null
                            }));
                    }
                    return candidates;                    
                })
            .Subscribe(Identification);

            ActivationObservable                
                .Take(1)
                .Select(_ => Vocabulary.getTerms(Svc.TermList.TaxonomicGroups))                
                .Subscribe(TaxonomicGroup);

            this.FirstActivation()
                .Select(_ => Vocabulary.getQualifications().ToList() as IList<Qualification>)
                .Subscribe(Qualifications);

            _IsToplevel
                .Where(isToplevel => !isToplevel)
                .Select(isToplevel => Vocabulary.getTerms(Svc.TermList.RelationshipTypes))                
                .Subscribe(RelationshipType);
            #endregion

            #region Preserve Selections
            
            Identification.ItemsObservable               
                .Where(x => x != null)
                .CombineLatest(ModelByVisitObservable.Where(m => m.IdentificationUri != null),
                (ids, model) => ids.FirstOrDefault(id => id.Selected.URI == Current.Model.IdentificationUri) ?? ids.FirstOrDefault())
                .Subscribe(x => Identification.SelectedItem = x);
            
            
            TaxonomicGroup.ItemsObservable                
                .Where(x => x != null)
                .CombineLatest(ModelByVisitObservable.Where(m => m.TaxonomicGroup != null),
                (tgs,m) => tgs.FirstOrDefault(tg => tg.Code == Current.Model.TaxonomicGroup))
                .Subscribe(x => TaxonomicGroup.SelectedItem = x);

            
            RelationshipType.ItemsObservable                
                .Where(x => x != null)
                .CombineLatest(ModelByVisitObservable.Where(m => m.RelationType != null),
                (rels,m) => rels.FirstOrDefault(rel => rel.Code == Current.Model.RelationType))
                .Where(x => x != null)
                .Subscribe(x => RelationshipType.SelectedItem = x);

            Qualifications.ItemsObservable
                .Where(x => x != null)
                .CombineLatest(ModelByVisitObservable.Where(m => m.Qualification != null),
                (qualis, m) => qualis.FirstOrDefault(q => q.Code == Current.Model.Qualification))
                .Where(x => x != null)
                .Subscribe(x => Qualifications.SelectedItem = x);
            #endregion          

            Messenger.RegisterMessageSource(
                Save                
                .Select(_ => TaxonomicGroup.SelectedItem),
                MessageContracts.USE);

            Messenger.RegisterMessageSource(
                Save
                .Select(_ => RelationshipType.SelectedItem),
                MessageContracts.USE);
        }
      
        private void registerCanSave()
        {
            var taxonomicGroupIsSet = this.TaxonomicGroup
                .Select(term => term != null)
                .StartWith(false);

            var identificationIsSelected = this.Identification
                .Select(id => id != null && !string.IsNullOrWhiteSpace(id.Selected.TaxonNameCache))
                .StartWith(false);
            
            taxonomicGroupIsSet.BooleanAnd(identificationIsSelected).Subscribe(CanSaveSubject.OnNext);
        }

        protected override void UpdateModel()
        {
            if (!Current.Model.IsLocalized())
                Current.Model.SetCoordinates(_latest_location.First());
            Current.Model.TaxonomicGroup = TaxonomicGroup.SelectedItem.Code;
            Current.Model.WorkingName = Identification.SelectedItem.Selected.TaxonNameCache;
            Current.Model.OnlyObserved = this.OnlyObserved;
            Current.Model.IdentificationUri = Identification.SelectedItem.Selected.URI;
            Current.Model.RelationType = (RelationshipType.SelectedItem != null) ? RelationshipType.SelectedItem.Code : null;
            Current.Model.Qualification = (Qualifications.SelectedItem != null) ? Qualifications.SelectedItem.Code : null;
            Current.Model.AnalysisDate = AnalysisDate;
        }
    }
}
