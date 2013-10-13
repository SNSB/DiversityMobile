using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DiversityPhone.ViewModels {
    public abstract class EditPageVMBase<T> : ElementPageVMBase<T>, IEditPageVM where T : IModifyable, IReactiveNotifyPropertyChanged {
        public IReactiveCommand Save { get; private set; }
        public IReactiveCommand ToggleEditable { get; private set; }
        public IReactiveCommand Delete { get; private set; }


        private ObservableAsPropertyHelper<bool> _IsEditable;
        /// <summary>
        /// Shows, whether the current Object can be Edited
        /// </summary>
        public bool IsEditable {
            get {
                return _IsEditable.Value;
            }
        }

        protected ISubject<bool> CanSaveSubject { get; private set; }
        private ISubject<Unit> DeleteSubject = new Subject<Unit>();

        public EditPageVMBase(Predicate<T> filter = null) {
            CanSaveSubject = new Subject<bool>();
            Save = new ReactiveCommand(CanSaveSubject);
            Save
                .Do(_ => UpdateModel())
                .Select(_ => Current)
                .Do(_ => Messenger.SendMessage(Page.Previous))
                .ToMessage(Messenger, MessageContracts.SAVE);

            ToggleEditable = new ReactiveCommand(ModelByVisitObservable.Select(m => !m.IsUnmodified()));
            _IsEditable = this.ObservableToProperty(
                    Observable.Merge(
                        ModelByVisitObservable
                        .Select(m => m.IsNew()),
                        ToggleEditable.Select(_ => !IsEditable)
                    ),
                x => x.IsEditable);

            Delete = new ReactiveCommand(ModelByVisitObservable.Select(m => !m.IsNew()));
            Delete
                .SelectMany(_ =>
                    Notifications.showDecision(DiversityResources.Message_ConfirmDelete)
                    .Where(x => x)
                    .Select(_2 => Current)
                    .Do(_2 => Messenger.SendMessage(Page.Previous))
                )
                .ToMessage(Messenger, MessageContracts.DELETE);


            Messenger.Listen<IElementVM<T>>(MessageContracts.EDIT)
                .Where(vm => vm != null)
                .Where(vm => filter == null || filter(vm.Model))
                .Subscribe(x => Current = x);
        }

        protected virtual void UpdateModel() { }
    }
}
