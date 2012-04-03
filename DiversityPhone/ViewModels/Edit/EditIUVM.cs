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

namespace DiversityPhone.ViewModels
{
    public class EditIUVM : EditElementPageVMBase<IdentificationUnit>
    {
        private Container IOC;
        private ITaxonService Taxa { get; set; }
        private IVocabularyService Vocabulary { get; set; }
        private IGeoLocationService Geolocation;        

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

        public bool IsToplevel { get { return _IsToplevel.Value; } }
        private ObservableAsPropertyHelper<bool> _IsToplevel;
        
        #endregion



        public EditIUVM(Container ioc)
            : base(false)
        {
            IOC = ioc;
            Taxa = ioc.Resolve<ITaxonService>();
            Vocabulary = ioc.Resolve<IVocabularyService>();
            Geolocation = ioc.Resolve<IGeoLocationService>();

            TaxonomicGroup = new ListSelectionHelper<Term>();
            RelationshipType = new ListSelectionHelper<Term>();
            Identification = new ListSelectionHelper<TaxonName>();

            registerCanSave();         

            #region Update View
            _IsToplevel = ValidModel
                            .Select(m => m.RelatedUnitID == null)
                            .ToProperty(this, x => x.IsToplevel);

            var isObservation =
                ValidModel
                .Select(m => Storage.getSpecimenByID(m.SpecimenID))
                .Where(spec => spec != null)
                .Select(spec => spec.IsObservation());

            isObservation
                .CombineLatest(
                    ValidModel
                    .Select(m => m.OnlyObserved)
                    .StartWith(false),
                    (isobs, onlyobs) => isobs || onlyobs)
                .BindTo(this, vm => vm.OnlyObserved);

            _IsObservation = isObservation
                .ToProperty(this, vm => vm.IsObservation);

            ValidModel
                .Select(m => m.WorkingName)
                .Where(wn => !string.IsNullOrWhiteSpace(wn))
                .BindTo(this, x => x.QueryString);
            #endregion

            #region Vocabulary
            
            this.ObservableForProperty(vm => vm.QueryString)
            .Value()
            .CombineLatest(TaxonomicGroup,
                (query, tg) =>
                {
                    return Taxa.getTaxonNames(tg, query);                        
                })
            .Select( candidates =>
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
                        } );
                    return candidates;                    
                })
            .Subscribe(Identification);


            DistinctStateObservable
                .Select(_ => Vocabulary.getTerms(Svc.TermList.TaxonomicGroups))                
                .Subscribe(TaxonomicGroup);

            _IsToplevel
                .Where(isToplevel => !isToplevel)
                .Select(isToplevel => Vocabulary.getTerms(Svc.TermList.RelationshipTypes))                
                .Subscribe(RelationshipType);
            #endregion

            #region Preserve Selections
            
            Identification.ItemsObservable               
                .Where(x => x != null)
                .CombineLatest(ValidModel.Where(m => m.IdentificationUri != null),
                (ids, model) => ids.FirstOrDefault(id => id.URI == Current.Model.IdentificationUri))                
                .BindTo(Identification, x => x.SelectedItem);
            
            
            TaxonomicGroup.ItemsObservable                
                .Where(x => x != null)  
                .CombineLatest(ValidModel.Where(m => m.TaxonomicGroup != null),
                (tgs,m) => tgs.FirstOrDefault(tg => tg.Code == Current.Model.TaxonomicGroup))
                .BindTo(TaxonomicGroup, x => x.SelectedItem);

            
            RelationshipType.ItemsObservable                
                .Where(x => x != null)
                .CombineLatest(ValidModel.Where(m => m.RelationType != null),
                (rels,m) => rels.FirstOrDefault(rel => rel.Code == Current.Model.RelationType))
                .Where(x => x != null)
                .BindTo(RelationshipType, x => x.SelectedItem);
            #endregion          

            Messenger.RegisterMessageSource(
                Save                
                .Select(_ => TaxonomicGroup.SelectedItem),
                MessageContracts.USE); 
        }       

        protected override IdentificationUnit ModelFromState(PageState state)
        {
            IdentificationUnit result = null;
            if (state.Context != null)
            {
                int id;
                if (int.TryParse(state.Context, out id))
                {
                    result = Storage.getIdentificationUnitByID(id);
                }
            }            
            if (result == null && state.ReferrerType != ReferrerType.None)
            {
                result = new IdentificationUnit();
                
                if (state.ReferrerType == ReferrerType.Specimen)
                {
                    int id;
                    if (int.TryParse(state.Referrer, out id))
                    {
                        result.SpecimenID = id;
                    }
                }
                else if (state.ReferrerType == ReferrerType.IdentificationUnit)
                {
                    int id;
                    IdentificationUnit parent;
                    if (int.TryParse(state.Referrer, out id)
                        && (parent = Storage.getIdentificationUnitByID(id)) != null)
                    {
                        result.SpecimenID = parent.SpecimenID;
                        result.RelatedUnitID = parent.UnitID;
                    }
                }
                Geolocation.fillGeoCoordinates(result);
            }

            return result;
        }   

        private void registerCanSave()
        {
            var taxonomicGroupIsSet = this.TaxonomicGroup
                .Select(term => term != null)
                .StartWith(false);

            var identificationIsSelected = this.Identification
                .Select(id => id != null)
                .StartWith(false);

            taxonomicGroupIsSet.BooleanAnd(identificationIsSelected).Subscribe(_CanSaveSubject);
        }

        protected override void UpdateModel()
        {
            Current.Model.TaxonomicGroup = TaxonomicGroup.SelectedItem.Code;
            Current.Model.WorkingName = Identification.SelectedItem.TaxonNameCache;
            Current.Model.OnlyObserved = this.OnlyObserved;
            Current.Model.IdentificationUri = Identification.SelectedItem.URI;
            Current.Model.RelationType = (RelationshipType.SelectedItem != null) ? RelationshipType.SelectedItem.Code : null;
        }
        
        protected override ElementVMBase<IdentificationUnit> ViewModelFromModel(IdentificationUnit model)
        {
            return new IdentificationUnitVM(IOC, model, Page.Current);
        }
    }
}
