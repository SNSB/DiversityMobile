using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System.Reactive.Subjects;

namespace DiversityPhone.ViewModels
{
    public class EditIUVM : PageViewModel
    {
        #region Services
        IMessageBus _messenger;
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
                return _TaxonomicGroups ?? (_TaxonomicGroups = _storage.getTerms((int) SourceID.TaxonomicGroup));
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


        public bool IsToplevel { get { return _IsToplevel.Value; } }
        private ObservableAsPropertyHelper<bool> _IsToplevel;
        #endregion



        public EditIUVM(IMessageBus messenger, IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;           

            var model = StateObservable                
                .Select(s => UnitFromState(s));


            ToggleEditable = new ReactiveCommand();
            

            _isEditable = StateObservable
                .Select(s => s.Context == null) //Newly created Units are immediately editable
                .Merge(
                    ToggleEditable.Select(_ => !Editable) //Toggle Editable
                )
                .ToProperty(this, vm => vm.Editable);

            _Model = model.ToProperty(this, x => x.Model);
                         
            _IsToplevel = model
                            .Select(m => m.RelatedUnitID == null)
                            .ToProperty(this, x => x.IsToplevel);

            var isObservation =
                model
                .Select(m => _storage.getSpecimenByID(m.SpecimenID))
                .Where(spec => spec != null)
                .Select(spec => spec.IsObservation());

            isObservation.BindTo(this, vm => vm.OnlyObserved);

            _IsObservation = isObservation                
                .ToProperty(this, vm => vm.IsObservation);



            

            model.Select(m => m.TaxonomicGroup)
                .Select(tg => TaxonomicGroups.FirstOrDefault(t => t.Code == tg) ?? ((TaxonomicGroups.Count > 0) ? TaxonomicGroups[0] : null))
                .BindTo(this, vm => vm.SelectedTaxGroup);

            Save = new ReactiveCommand(validationObservable());
            _messenger.RegisterMessageSource(
                Save
                .Do(_ => updateModel())
                .Select(_ => Model),
                MessageContracts.SAVE);

            _messenger.RegisterMessageSource(
                Delete
                .Select(_ => Model),
                MessageContracts.DELETE);                

            _messenger.RegisterMessageSource(
                Delete
                .Merge(Save)
                .Select(_ => Message.NavigateBack)
                );
            
        }       

        private IdentificationUnit UnitFromState(PageState state)
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
            if (result != null && state.ReferrerType != ReferrerType.None)
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
                    if (int.TryParse(state.Context, out id) 
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
        }
    }
}
