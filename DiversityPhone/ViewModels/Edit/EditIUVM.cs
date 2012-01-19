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

namespace DiversityPhone.ViewModels
{
    public class EditIUVM : ElementPageViewModel<IdentificationUnit>
    {
        #region Services        
        IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand ToggleEditable { get; private set; }
        public ReactiveCommand Delete { get; private set; }
        #endregion

        #region Properties
        
        private ObservableAsPropertyHelper<bool> _IsEditable;
        public bool Editable
        {
            get
            {
                return _IsEditable.Value;
            }
        }

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
        

        public IdentificationUnit Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<IdentificationUnit> _Model;


        private ObservableAsPropertyHelper<IList<Term>> _TaxonomicGroups;
        public IList<Term> TaxonomicGroups
        {
            get
            {
                return _TaxonomicGroups.Value;
            }
        }


        private Term _SelectedTaxGroup = null;
        public Term SelectedTaxGroup
        {
            get
            {
                return _SelectedTaxGroup;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SelectedTaxGroup, ref _SelectedTaxGroup, value);
            }
        }

        private ObservableAsPropertyHelper<IList<Term>> _RelationshipTypes;
        public IList<Term> RelationshipTypes
        {
            get
            {
                return _RelationshipTypes.Value;
            }
        }


        private Term _SelectedRelationshipType;
        public Term SelectedRelationshipType
        {
            get
            {
                return _SelectedRelationshipType;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SelectedRelationshipType, ref _SelectedRelationshipType, value);
            }
        }
        


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


        private string _Genus;
        public string Genus
        {
            get
            {
                return _Genus;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Genus, ref _Genus, value);
            }
        }

        
        private string _Species;
        public string Species
        {
            get
            {
                return _Species;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Species, ref _Species, value);
            }
        }

        private ObservableAsPropertyHelper<IEnumerable<TaxonName>> _AvailableIdentifications;
        public IEnumerable<TaxonName> AvailableIdentifications
        {
            get
            {
                return _AvailableIdentifications.Value;
            }
        }


        private TaxonName _CurrentIdentification;
        public TaxonName CurrentIdentification
        {
            get
            {
                return _CurrentIdentification;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.CurrentIdentification, ref _CurrentIdentification, value);
            }
        }
        
        
        


        public bool IsToplevel { get { return _IsToplevel.Value; } }
        private ObservableAsPropertyHelper<bool> _IsToplevel;
        #endregion



        public EditIUVM(IMessageBus messenger, IOfflineStorage storage)
            : base(messenger, false)
        {
            
            _storage = storage;

            ToggleEditable = new ReactiveCommand();
            Save = new ReactiveCommand(validationObservable());
            Delete = new ReactiveCommand();

            #region Update View
            _IsEditable = DistinctStateObservable
                .Select(s => s.Context == null) //Newly created Units are immediately editable
                .Merge(
                    ToggleEditable.Select(_ => !Editable) //Toggle Editable
                )
                .ToProperty(this, vm => vm.Editable);


            _Model = ValidModel.ToProperty(this, x => x.Model);

            _IsToplevel = ValidModel
                            .Select(m => m.RelatedUnitID == null)
                            .ToProperty(this, x => x.IsToplevel);

            var isObservation =
                ValidModel
                .Select(m => _storage.getSpecimenByID(m.SpecimenID))
                .Where(spec => spec != null)
                .Select(spec => spec.IsObservation());

            isObservation.BindTo(this, vm => vm.OnlyObserved);

            _IsObservation = isObservation
                .ToProperty(this, vm => vm.IsObservation);





            #endregion

            #region Vocabulary
            _AvailableIdentifications = Observable.CombineLatest(
                this.ObservableForProperty(vm => vm.Genus)
                .Select(g => g.Value)
                .StartWith(String.Empty),
                this.ObservableForProperty(vm => vm.Species)
                .Select(s => s.Value)
                .StartWith(String.Empty),
                (g, s) => new { Genus = g, Species = s })
                .Select(query =>
                    {
                        //Don't allow short queries
                        if (query.Species.Length + query.Genus.Length > 3)
                            return _storage.getTaxonNames(SelectedTaxGroup, query.Genus, query.Species);
                        else
                            return Enumerable.Empty<TaxonName>();
                    })
                .Select( candidates =>                     
                    //Append WorkingName as Identification
                    candidates.Concat(
                    new[] { 
                        new TaxonName() 
                        { 
                            TaxonNameCache = Genus + " " + Species,
                            GenusOrSupragenic = Genus, 
                            SpeciesEpithet = Species, 
                            Synonymy = DiversityPhone.Model.Synonymy.WorkingName,
                            URI = null,
                        } 
                    }))
                .ToProperty(this, vm => vm.AvailableIdentifications);


            _TaxonomicGroups = DistinctStateObservable
                .Select(_ => _storage.getTerms(Svc.TermList.TaxonomicGroups))
                .ToProperty(this, vm => vm.TaxonomicGroups);

            _RelationshipTypes = _IsToplevel
                .Where(isToplevel => isToplevel)
                .Select(isToplevel => _storage.getTerms(Svc.TermList.RelationshipTypes))
                .ToProperty(this, vm => vm.RelationshipTypes);
            #endregion

            #region Preserve Selections
            
            this.ObservableForProperty(x => x.AvailableIdentifications)
                .Select(change => change.Value)
                .Where(x => x != null)
                .Select(ids => (Model != null) ? ids.FirstOrDefault(id => id.URI == Model.IdentificationUri) : null)
                .BindTo(this, vm => vm.CurrentIdentification);

            
            this.ObservableForProperty(x => x.TaxonomicGroups)
                .Select(change => change.Value)
                .Where(x => x != null)
                .Select(tgs => ((Model != null) ? tgs.FirstOrDefault(tg => tg.Code == Model.TaxonomicGroup) : null ) ?? tgs.FirstOrDefault())
                .BindTo(this, vm => vm.SelectedTaxGroup);

            
            this.ObservableForProperty(x => x.RelationshipTypes)
                .Select(change => change.Value)
                .Where(x => x != null)
                .Select(rels => ((Model != null) ? rels.FirstOrDefault(rel => rel.Code == Model.RelationType) : null) ?? rels.FirstOrDefault())
                .BindTo(this, x => x.SelectedRelationshipType);
            #endregion


            Messenger.RegisterMessageSource(
                Save
                .Do(_ => updateModel())
                .Select(_ => Model),
                MessageContracts.SAVE);

            Messenger.RegisterMessageSource(
                Save                
                .Select(_ => SelectedTaxGroup),
                MessageContracts.USE);

            Messenger.RegisterMessageSource(
                Delete
                .Select(_ => Model),
                MessageContracts.DELETE);                

            Messenger.RegisterMessageSource(
                Delete
                .Merge(Save)                
                .Select(_ => Message.NavigateBack)
                );

        }       

        protected override IdentificationUnit ModelFromState(PageState state)
        {
            IdentificationUnit result = null;
            if (state.Context != null)
            {
                int id;
                if (int.TryParse(state.Context, out id))
                {
                    result = _storage.getIdentificationUnitByID(id);
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
                        && (parent = _storage.getIdentificationUnitByID(id)) != null)
                    {
                        result.SpecimenID = parent.SpecimenID;
                        result.RelatedUnitID = parent.UnitID;
                    }
                }
            }

            return result;
        }        


        private IObservable<bool> validationObservable()
        {
            var taxonomicGroupIsSet = this.ObservableForProperty(x => x.SelectedTaxGroup)
                .Select(term => term.Value != null)
                .StartWith(false);

            var identificationIsSelected = this.ObservableForProperty(x => x.CurrentIdentification)
                .Select(id => id.Value != null)
                .StartWith(false);

            return taxonomicGroupIsSet.BooleanAnd(identificationIsSelected);
        }

        private void updateModel()
        {
            Model.TaxonomicGroup = SelectedTaxGroup.Code;
            Model.WorkingName = (CurrentIdentification.Synonymy == Synonymy.WorkingName) ? CurrentIdentification.TaxonNameCache : null;
            Model.OnlyObserved = this.OnlyObserved;
            Model.IdentificationUri = CurrentIdentification.URI;
            Model.RelationType = (SelectedRelationshipType != null) ? SelectedRelationshipType.Code : null;
        }
    }
}
