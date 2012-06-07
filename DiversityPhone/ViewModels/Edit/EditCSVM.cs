using System;
using System.Reactive.Linq;
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class EditCSVM : EditElementPageVMBase<Specimen>
    {
        #region Properties
        private string _AccessionNumber;
        public string AccessionNumber
        {
            get { return _AccessionNumber; }
            set { this.RaiseAndSetIfChanged(x => x.AccessionNumber, ref _AccessionNumber, value); }
        }
        #endregion


        public EditCSVM()
            
        {    
            //Read-Only Eigenschaften direkt ans Model Binden
            //Nur Veränderbare Properties oder abgeleitete so binden
            ValidModel                    
                .Select(m => m.AccessionNumber != null ? m.AccessionNumber : "")
                .BindTo(this, x=>x.AccessionNumber);       
        }

        protected override IObservable<bool> CanSave()
        {
            IObservable<bool> accessionNumber = this.ObservableForProperty(x => x.AccessionNumber)
                .Select(desc => !string.IsNullOrWhiteSpace(desc.Value))
                .StartWith(false);
            IObservable<bool> canSave = accessionNumber;
            return canSave;
        }

        protected override void UpdateModel()
        {
            //Nur Veränderbare Eigenschaften übernehmen.
            Current.Model.AccessionNumber = AccessionNumber;
        }

        protected override Specimen ModelFromState(PageState s)
        {
            if (s.Context != null)
            {
                int id;
                if (int.TryParse(s.Context, out id))
                {
                    return Storage.getSpecimenByID(id);
                }
            }
            else if (s.Referrer != null && s.ReferrerType == ReferrerType.Event)
            {
                int parentId;
                if (int.TryParse(s.Referrer, out parentId))
                {
                    return new Specimen()
                        {
                            CollectionEventID = parentId,                            
                        };
                }
            }
            return null;
        }

        protected override ElementVMBase<Specimen> ViewModelFromModel(Specimen model)
        {
            return new SpecimenVM(model);
        }
    }
}
