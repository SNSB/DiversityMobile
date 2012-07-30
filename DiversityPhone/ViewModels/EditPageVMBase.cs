using ReactiveUI;
using System.Reactive.Linq;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{
    public abstract class EditPageVMBase<T> : ElementPageVMBase<T>
    {
        public EditPageVMBase()
        {
            Messenger.Listen<IElementVM<T>>(MessageContracts.EDIT)
                .Where(vm => vm != null)
                .BindTo(this, x => x.Current);
        }
    }
}
