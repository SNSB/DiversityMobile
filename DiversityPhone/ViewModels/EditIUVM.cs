using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;

namespace DiversityPhone.ViewModels
{
    public class EditIUVM : ReactiveObject
    {
        IMessageBus _messenger;
        IOfflineStorage _storage;

        #region Properties

        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand Cancel { get; private set; }
        
        public IdentificationUnit Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<IdentificationUnit> _Model;        
        

        private string _AccessionNumber;

        public string AccessionNumber
        {
            get
            {
                return _AccessionNumber;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.AccessionNumber,ref _AccessionNumber, value);
            }
        }        


        public IList<Term> _TaxonomicGroups = null;
        public IList<Term> TaxonomicGroups
        {
            get
            {
                return _TaxonomicGroups ?? (_TaxonomicGroups = _storage.getTerms(0));
            }            
        }

        
        public int _SelectedTaxGroup = -1; 

        public int SelectedTaxGroup
        {
            get
            {
                return _SelectedTaxGroup;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SelectedTaxGroup,ref _SelectedTaxGroup, value);
            }
        }        

        public bool IsToplevel { get { return _IsToplevel.Value; } }
        private ObservableAsPropertyHelper<bool> _IsToplevel;

        #endregion



        public EditIUVM(IMessageBus messenger, IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;

            var model = _messenger.Listen<IdentificationUnit>(MessageContracts.EDIT);
            model.Select(m => m.AccessionNumber)
                .BindTo(this, x => x.AccessionNumber);
            model.Select(m => m.TaxonomicGroup)
                .Select(tg => string.IsNullOrEmpty(tg) ? -1 : TaxonomicGroups.ListFindIndex(t => t.Code == tg))
                .BindTo(this, x => x.SelectedTaxGroup);                                        
            _Model = model.ToProperty(this, x => x.Model);

            var isToplevel = model
                .Select(m => m.RelatedUnitID == null);
            _IsToplevel = isToplevel.ToProperty(this, x => x.IsToplevel);

            var canSave = this.ObservableForProperty(x => x.SelectedTaxGroup)
                                .Select(change => change.Value > -1).StartWith(false);
                
            


            (Cancel = new ReactiveCommand())
                .Subscribe(_ => _messenger.SendMessage<Message>(Message.NavigateBack));

            (Save = new ReactiveCommand(canSave))
                .Subscribe(_ =>
                    {
                        updateModel();
                        _messenger.SendMessage<IdentificationUnit>(Model, MessageContracts.SAVE);
                    });
        }        

        private void updateModel()
        {

        }


    }
}
