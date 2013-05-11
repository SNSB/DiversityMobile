using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Linq;
using System;
using System.Reactive.Concurrency;
using System.Diagnostics.Contracts;
using System.Reactive;
using System.Threading;

namespace DiversityPhone.ViewModels
{
    public interface ISelectable<T>
    {
        T Value { get; }
        bool IsSelected { get; set; }
    }

    public class MultipleSelectionHelper<T> : ReactiveObject, IObserver<IObservable<T>>
    {
        public readonly IScheduler Dispatcher;

        private class Selectable<U> : ReactiveObject, ISelectable<U>
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

        readonly ISubject<IObservable<T>> _ItemStreams = new Subject<IObservable<T>>();

        public MultipleSelectionHelper(
            [Dispatcher] IScheduler Dispatcher = null
            )
        {
            Dispatcher = Dispatcher ?? DispatcherScheduler.Current;
            this.Dispatcher = Dispatcher;

            Items = new ReactiveCollection<T>();


            _SelectableItems = Items.CreateDerivedCollection(x => new Selectable<T>(this, x) as ISelectable<T>);

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
