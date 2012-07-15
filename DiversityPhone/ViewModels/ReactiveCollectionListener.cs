using System;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{
    public class ListeningReactiveCollection<T> : ReactiveCollection<T>
    {
        CompositeDisposable _subscription = new CompositeDisposable();

        private IMessageBus _Messenger;

        public IMessageBus Messenger
        {
            get { return _Messenger; }
            set 
            {
                if(_Messenger != value)
                {
                _Messenger = value; 
                _subscription.Clear();  
                    if(_Messenger != null)
                    {
                        _subscription.Add(
                            _Messenger.Listen<T>(MessageContracts.SAVE)
                            .Subscribe(this.Add)
                            );
                        _subscription.Add(
                            _Messenger.Listen<T>(MessageContracts.DELETE)
                            .Subscribe(i => this.Remove(i))
                            );
                    }
                }
            }
        }

        public ListeningReactiveCollection(IEnumerable<T> collection = null)
            : base(collection ?? Enumerable.Empty<T>())
        {
            Messenger = MessageBus.Current;
        }

    }
}
