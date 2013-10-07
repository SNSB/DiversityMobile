using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DiversityPhone.ViewModels
{
    public interface ISelectable<T>
    {
        T Value { get; }
        bool IsSelected { get; set; }
    }


    /// <summary>
    /// Helper Class for working with a Collection and selecting multiple Items from it.
    /// The Collection is supplied via an IObservable containing an IObservable<T>
    /// an asynchronous stream of Items that are accumulated into a Collection
    /// </summary>
    /// <remarks>
    /// Can marshal new Items to the Dispatcher Thread for UI safety
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class MultipleSelectionHelper<T> : ReactiveObject, IObserver<IObservable<T>>
    {
        public readonly IScheduler Dispatcher;
        private readonly ISubject<IObservable<T>> _ItemStreams = new Subject<IObservable<T>>();

        /// <summary>
        /// Internal Class to encapsulate a single Item and store its selection status
        /// </summary>
        /// <remarks>
        /// Needs to be public to enable Binding in Windows Phone 7
        /// </remarks>
        /// <typeparam name="U"></typeparam>
        public class Selectable<U> : ReactiveObject, ISelectable<U>
        {
            public U Value { get; private set; }

            private bool _IsSelected;
            public bool IsSelected
            {
                get
                {
                    return _IsSelected;
                }
                set
                {
                    this.RaiseAndSetIfChanged(x => x.IsSelected, ref _IsSelected, value);
                }
            }

            public Selectable(MultipleSelectionHelper<U> owner, U value)
            {
                Value = value;
                IsSelected = false;
            }
        }

        private ReactiveCollection<T> Items { get; set; }

        ReactiveDerivedCollection<ISelectable<T>> _SelectableItems;
        public IEnumerable<ISelectable<T>> SelectableItems { get { return _SelectableItems; } }

        public bool IsSelecting { get; set; }

        public IObservable<IEnumerable<T>> SelectedItems
        {
            get
            {
                return Observable.Defer(() =>
                   Observable.Start<IEnumerable<T>>(() => (from i in _SelectableItems
                                                           where !IsSelecting || i.IsSelected
                                                           select i.Value).ToList(),
                                                           Dispatcher));
            }
        }

        public IObservable<IEnumerable<T>> UnSelectedItems
        {
            get
            {
                return Observable.Defer(() =>
                    Observable.Start<IEnumerable<T>>(() => (from i in _SelectableItems
                                                            where IsSelecting && !i.IsSelected
                                                            select i.Value).ToList(),
                                                            Dispatcher));
            }
        }

        /// <summary>
        /// Constructs a new MultipleSelectionHelper
        /// </summary>
        /// <param name="Dispatcher">
        /// The IScheduler Instance that publicly visible change notifications are marshalled to.
        /// Default: Use the current thread's Dispatcher.
        /// </param>
        public MultipleSelectionHelper(
            [Dispatcher] IScheduler Dispatcher = null
            )
        {
            Dispatcher = Dispatcher ?? DispatcherScheduler.Current;
            this.Dispatcher = Dispatcher;

            Items = new ReactiveCollection<T>();

            _SelectableItems = Items.CreateDerivedCollection(x => new Selectable<T>(this, x) as ISelectable<T>);

            _SelectableItems
                .CollectionCountChanged
                .Select(count => count == 0)
                .Where(isEmpty => isEmpty)
                .Subscribe(_ =>
                    IsSelecting = false
                    );

            _ItemStreams
                .ObserveOn(Dispatcher)
                .Do(_ => Items.Clear())
                .Select(stream => stream.TakeUntil(_ItemStreams))
                .Switch()
                .ObserveOn(Dispatcher)
                .Subscribe(Items.Add);
        }

        public void Add(T item)
        {
            Dispatcher.Schedule(() => Items.Add(item));
        }

        public void Remove(T item)
        {
            Dispatcher.Schedule(() => Items.Remove(item));
        }

        public void OnCompleted()
        {
            _ItemStreams.OnCompleted();
        }

        public void OnError(Exception exception)
        {
            _ItemStreams.OnError(exception);
        }

        public void OnNext(IObservable<T> value)
        {
            _ItemStreams.OnNext(value);
        }
    }
}
