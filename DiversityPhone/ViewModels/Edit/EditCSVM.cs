﻿using System;
using System.Reactive.Linq;
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class EditCSVM : EditPageVMBase<Specimen>
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
            ModelByVisitObservable                    
                .Select(m => m.AccessionNumber != null ? m.AccessionNumber : "")
                .BindTo(this, x=>x.AccessionNumber);

            CanSave().Subscribe(CanSaveSubject.OnNext);
        }

        protected IObservable<bool> CanSave()
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
    }
}
