namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    public class EditCSVM : EditPageVMBase<Specimen>
    {
        #region Properties

        private string _AccessionNumber;

        public string AccessionNumber
        {
            get { return _AccessionNumber; }
            set { this.RaiseAndSetIfChanged(x => x.AccessionNumber, ref _AccessionNumber, value); }
        }

        #endregion Properties

        public EditCSVM(IMessageBus Messenger, INotificationService Notifications)
        {
            ModelByVisitObservable
                .Select(m => m.AccessionNumber != null ? m.AccessionNumber : "")
                .Subscribe(x => AccessionNumber = x);

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

        protected override async Task UpdateModel()
        {
            Current.Model.AccessionNumber = AccessionNumber;
        }
    }
}