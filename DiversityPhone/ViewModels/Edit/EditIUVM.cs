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
        IList<IDisposable> _subscriptions;

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
        public bool IsEditable
        {
            get
            {
                return _isEditable.Value;
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
                .Select(s => UnitFromContext(s.Context));


            ToggleEditable = new ReactiveCommand();
            

            _isEditable = StateObservable
                .Select(s => s.Context == null) //Newly created Units are immediately editable
                .Merge(
                    ToggleEditable.Select(_ => !IsEditable) //Toggle Editable
                )
                .ToProperty(this, vm => vm.IsEditable);

            _Model = model.ToProperty(this, x => x.Model);


            var isToplevel = model
                .Select(m => m.RelatedUnitID == null);
            _IsToplevel = isToplevel.ToProperty(this, x => x.IsToplevel);

            var canSave = validationObservable();

            _subscriptions = new List<IDisposable>()
            {            
                

                model.Select(m => m.TaxonomicGroup)
                    .Select(tg => TaxonomicGroups.FirstOrDefault(t => t.Code == tg) ?? ((TaxonomicGroups.Count > 0) ? TaxonomicGroups[0] : null))
                    .Subscribe(x => SelectedTaxGroup = x),

                (Save = new ReactiveCommand(canSave))
                    .Subscribe(_ =>
                        {
                            updateModel();
                            _messenger.SendMessage<IdentificationUnit>(Model, MessageContracts.SAVE);                            
                            _messenger.SendMessage<Message>(Message.NavigateBack);
                        }),

                (Delete = new ReactiveCommand())
                    .Subscribe(_=> delete()),
            };
        }

        private void delete()
        {
            _messenger.SendMessage<IdentificationUnit>(Model, MessageContracts.DELETE);
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }

        private IdentificationUnit UnitFromContext(string ctx)
        {
            if (ctx != null)
            {
                int id;
                if (int.TryParse(ctx, out id))
                {
                    return _storage.getIdentificationUnitByID(id);
                }
            }
            return new IdentificationUnit();
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
