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
    public class EditIUVM : EditElementPageVMBase<IdentificationUnit>
    {
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



        public EditIUVM()
            : base(false)
        {
            #region Update View
            _IsToplevel = ValidModel
                            .Select(m => m.RelatedUnitID == null)
                            .ToProperty(this, x => x.IsToplevel);

            var isObservation =
                ValidModel
                .Select(m => Storage.getSpecimenByID(m.SpecimenID))
                .Where(spec => spec != null)
                .Select(spec => spec.IsObservation());

            isObservation.BindTo(this, vm => vm.OnlyObserved);

            _IsObservation = isObservation
                .ToProperty(this, vm => vm.IsObservation);
            #endregion

            #region Vocabulary
            _AvailableIdentifications = 
                this.ObservableForProperty(vm => vm.QueryString)
                .Value()
                .Select(query =>
                    {
                        return Storage.getTaxonNames(SelectedTaxGroup, query);
                        
                    })
                .Select( candidates =>                     
                    //Append WorkingName as Identification
                    candidates.Concat(
                    new[] { 
                        new TaxonName() 
                        { 
                            TaxonNameCache = QueryString,
                            GenusOrSupragenic = null, 
                            SpeciesEpithet = null, 
                            Synonymy = DiversityPhone.Model.Synonymy.WorkingName,
                            URI = null,
                        } 
                    }))
                .ToProperty(this, vm => vm.AvailableIdentifications);


            _TaxonomicGroups = DistinctStateObservable
                .Select(_ => Storage.getTerms(Svc.TermList.TaxonomicGroups))                
                .ToProperty(this, vm => vm.TaxonomicGroups);

            _RelationshipTypes = _IsToplevel
                .Where(isToplevel => !isToplevel)
                .Select(isToplevel => Storage.getTerms(Svc.TermList.RelationshipTypes))                
                .ToProperty(this, vm => vm.RelationshipTypes);
            #endregion

            #region Preserve Selections
            
            this.ObservableForProperty(x => x.AvailableIdentifications)
                .Select(change => change.Value)
                .Where(x => x != null)
                .Select(ids => ((Current != null && Current.Model.IdentificationUri != null) ? ids.FirstOrDefault(id => id.URI == Current.Model.IdentificationUri) : null))                
                .BindTo(this, vm => vm.CurrentIdentification);
            
            
            this.ObservableForProperty(x => x.TaxonomicGroups)
                .Select(change => change.Value)
                .Where(x => x != null)
                .Select(tgs => ((Current != null) ? tgs.FirstOrDefault(tg => tg.Code == Current.Model.TaxonomicGroup) : null ) ?? tgs.FirstOrDefault())
                .BindTo(this, vm => vm.SelectedTaxGroup);

            
            this.ObservableForProperty(x => x.RelationshipTypes)
                .Select(change => change.Value)
                .Where(x => x != null)
                .Select(rels => ((Current != null) ? rels.FirstOrDefault(rel => rel.Code == Current.Model.RelationType) : null) ?? rels.FirstOrDefault())
                .Where(x => x != null)
                .BindTo(this, x => x.SelectedRelationshipType);
            #endregion          

            Messenger.RegisterMessageSource(
                Save                
                .Select(_ => SelectedTaxGroup),
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
            }

            return result;
        }        


        protected override IObservable<bool> CanSave()
        {
            var taxonomicGroupIsSet = this.ObservableForProperty(x => x.SelectedTaxGroup)
                .Select(term => term.Value != null)
                .StartWith(false);

            var identificationIsSelected = this.ObservableForProperty(x => x.CurrentIdentification)
                .Select(id => id.Value != null)
                .StartWith(false);

            return taxonomicGroupIsSet.BooleanAnd(identificationIsSelected);
        }

        protected override void UpdateModel()
        {
            Current.Model.TaxonomicGroup = SelectedTaxGroup.Code;
            Current.Model.WorkingName = CurrentIdentification.TaxonNameCache;
            Current.Model.OnlyObserved = this.OnlyObserved;
            Current.Model.IdentificationUri = CurrentIdentification.URI;
            Current.Model.RelationType = (SelectedRelationshipType != null) ? SelectedRelationshipType.Code : null;
        }
        
        protected override ElementVMBase<IdentificationUnit> ViewModelFromModel(IdentificationUnit model)
        {
            return new IdentificationUnitVM(Messenger, model, Page.Current);
        }
    }
}
