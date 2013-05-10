using System.Reactive.Linq;
using System;
using DiversityPhone.Model;

namespace DiversityPhone.ViewModels
{
    public abstract class ViewPageVMBase<T> : ElementPageVMBase<T>
    {
        public ViewPageVMBase(Predicate<T> filter = null)
        {
            Messenger.Listen<IElementVM<T>>(MessageContracts.VIEW)
                .Where(vm => vm != null && vm.Model != null)
                .Where(vm => filter == null || filter(vm.Model))
                .Subscribe(x => Current = x);

            //If the current element has been deleted in the meantime, navigate back.
            Observable.CombineLatest(
                this.ActivationObservable
                .Select(active => active ? Current : null),
                Messenger.Listen<IElementVM<T>>(MessageContracts.DELETE),
                (current, deleted) => current == deleted
            )
                .Where(current_deleted => current_deleted)
                .Select(_ => Page.Previous)
                .ToMessage(); 
        }
    }
}
