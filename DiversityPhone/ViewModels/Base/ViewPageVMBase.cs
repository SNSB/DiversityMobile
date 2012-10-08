using ReactiveUI;
using System.Reactive.Linq;
using DiversityPhone.Messages;
using System;

namespace DiversityPhone.ViewModels
{
    public abstract class ViewPageVMBase<T> : ElementPageVMBase<T>
    {
        public ViewPageVMBase(Predicate<T> filter = null)
        {
            Messenger.Listen<IElementVM<T>>(MessageContracts.VIEW)
                .Where(vm => vm != null && vm.Model != null)
                .Where(vm => filter == null || filter(vm.Model))
                .BindTo(this, x => x.Current);
        }
    }
}
