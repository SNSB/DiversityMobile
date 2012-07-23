using System;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{    
    
    public static class ReactiveCollectionListenerMixin
    {
        public static IDisposable ListenToChanges<T>(this ReactiveCollection<T> This, Func<T,bool> filter = null)
        {
            if (This == null)
                throw new ArgumentNullException("This");
            if (filter == null)
                filter = (a) => true;

            var messenger = MessageBus.Current;

            return new CompositeDisposable(
                   messenger.Listen<T>(MessageContracts.SAVE)
                        .Where(filter)
                       .Subscribe(This.Add),
                   messenger.Listen<T>(MessageContracts.DELETE)
                        .Where(filter)
                       .Subscribe(i => This.Remove(i))
                   );
        }
    }
}
