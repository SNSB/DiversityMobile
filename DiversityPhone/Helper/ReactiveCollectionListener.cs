using System;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using DiversityPhone.Model;

namespace DiversityPhone.ViewModels
{    
    
    public static class ReactiveCollectionListenerMixin
    {
        public static IDisposable ListenToChanges<T,VM>(this ReactiveCollection<VM> This, Func<T,bool> filter = null) where VM : class, IElementVM<T>
        {
            if (This == null)
                throw new ArgumentNullException("This");
            if (filter == null)
                filter = (a) => true;

            var messenger = MessageBus.Current;

            return new CompositeDisposable(
                   messenger.Listen<IElementVM<T>>(MessageContracts.SAVE)                        
                        .Where(vm => filter(vm.Model))
                        .Select(vm => vm as VM)
                        .Where(vm => vm != null && !This.Contains(vm))
                       .Subscribe(This.Add),
                   messenger.Listen<IElementVM<T>>(MessageContracts.DELETE)
                        .Where(vm => filter(vm.Model))
                        .Select(vm => vm as VM)
                        .Where(vm => vm != null)
                       .Subscribe(i => This.Remove(i))
                   );
        }
    }
}
