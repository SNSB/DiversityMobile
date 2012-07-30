using ReactiveUI;
using System.Reactive.Linq;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{
    public abstract class ViewPageVMBase<T> : ElementPageVMBase<T>
    {
        public ViewPageVMBase()
        {
            Messenger.Listen<IElementVM<T>>(MessageContracts.VIEW)
                .Where(vm => vm != null && vm.Model != null)
                .BindTo(this, x => x.Current);
        }
    }
}
