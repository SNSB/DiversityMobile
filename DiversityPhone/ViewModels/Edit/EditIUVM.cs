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
        
        private ObservableAsPropertyHelper<bool> _isEditable;
        public bool Editable
        {
            get
            {
                return _isEditable.Value;
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


        private IList<Term> _TaxonomicGroups = null;
        public IList<Term> TaxonomicGroups
        {
            get
            {
                return _TaxonomicGroups ?? (_TaxonomicGroups = _storage.getTerms(Svc.TermList.TaxonomicGroups));
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

        private IList<Term> _RelationshipTypes = null;
        public IList<Term> RelationshipTypes
        {
            get
            {
                return _RelationshipTypes ?? (_RelationshipTypes = _storage.getTerms(Svc.TermList.RelationshipTypes));
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
                            GenusOrSupragenic = Genus, 
                            SpeciesEpithet = Species, 
                            Synonymy = DiversityPhone.Model.Synonymy.WorkingName 
                        } 
                    }))
                .ToProperty(this, vm => vm.AvailableIdentifications);

                                        


            #region Update View           
            _isEditable = StateObservable
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
         


            ValidModel
                .Select(m => m.TaxonomicGroup)
                .Select(tg => TaxonomicGroups.FirstOrDefault(t => t.Code == tg) ?? ((TaxonomicGroups.Count > 0) ? TaxonomicGroups[0] : null))
                .BindTo(this, vm => vm.SelectedTaxGroup);

            ValidModel
                .Select(m => m.RelationType)
                .Select(reltype => RelationshipTypes.FirstOrDefault(rt => rt.Code == reltype))
                .BindTo(this, vm => vm.SelectedRelationshipType);


            #endregion

            Messenger.RegisterMessageSource(
                Save
                .Do(_ => updateModel())
                .Select(_ => Model),
                MessageContracts.SAVE);

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
                .Select(term => term != null)
                .StartWith(false);           

            return taxonomicGroupIsSet;
        }

        private void updateModel()
        {
            Model.TaxonomicGroup = SelectedTaxGroup.Code;
            Model.RelationType = SelectedRelationshipType.Code;
        }
    }
}
