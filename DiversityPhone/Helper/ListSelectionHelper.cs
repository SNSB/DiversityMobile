namespace DiversityPhone.ViewModels {
    using ReactiveUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public interface IListSelector<T> : IReactiveNotifyPropertyChanged {
        IList<T> Items { get; }
        int SelectedIndex { get; set; }
        T SelectedItem { get; set; }

        IObserver<IList<T>> ItemsObserver { get; }
        IObservable<IList<T>> ItemsObservable { get; }
        IObservable<T> SelectedItemObservable { get; }
    }

    public class ListSelectionHelper<T> : ReactiveObject, IListSelector<T> {
        private int deferredIndex = -1;
        private bool _UpdatingItems;
        private bool UpdatingItems {
            get {
                return _UpdatingItems;
            }
            set {
                _UpdatingItems = value;
                if (!_UpdatingItems)
                    SelectedIndex = deferredIndex;
                if (_UpdatingItems)
                    deferredIndex = SelectedIndex;
            }
        }

        private IList<T> _Items = new List<T>();
        public IList<T> Items {
            get {
                return _Items;
            }
            private set {
                this.RaiseAndSetIfChanged(x => x.Items, ref _Items, value ?? new List<T>());
            }
        }
        public IObservable<IList<T>> ItemsObservable { get; private set; }


        private int _SelectedIndex = -1;
        public int SelectedIndex {
            get {
                return _SelectedIndex;
            }
            set {
                if (UpdatingItems)
                    deferredIndex = value;
                else {
                    _SelectedIndex = value;

                    _SelectedItem = (value > -1) ? Items[value] : default(T);

                    this.RaisePropertyChanged(x => x.SelectedIndex);
                    this.RaisePropertyChanged(x => x.SelectedItem);
                    _SelectedItemSubject.OnNext(_SelectedItem);
                }
            }
        }


        private ISubject<IList<T>> _ItemsSubject;
        private ISubject<T> _SelectedItemSubject;

        private T _SelectedItem;
        public T SelectedItem {
            get {
                return _SelectedItem;
            }
            set {
                correctSelectedIndex(Items, value);
            }
        }

        public IObservable<T> SelectedItemObservable { get; private set; }

        public IObserver<IList<T>> ItemsObserver { get; private set; }

        public ListSelectionHelper(IScheduler Scheduler) {
            Scheduler = Scheduler ?? DefaultScheduler.Instance;

            _ItemsSubject = new ScheduledSubject<IList<T>>(Scheduler);
            ItemsObserver = _ItemsSubject;

            var itemsObservable = _ItemsSubject
                .Do(items => {
                    UpdatingItems = true;
                    var emptySelection = (items != null && !items.Any()) || (items == null);

                    try {
                        Items = items;
                    }
                    catch (InvalidOperationException)
                        // Exception thrown by the bound ListBox Control when swapping the items list with empty selection
                        // Empty Selection is not technically supported ( SelectedIndex == -1 && SelectedItem == null)
                    {
                        if (!emptySelection)
                            throw;
                    }

                    correctSelectedIndex(items, SelectedItem);
                    UpdatingItems = false;
                })
                .Publish();

            ItemsObservable = itemsObservable;
            itemsObservable.Connect();

            _SelectedItemSubject = new ReplaySubject<T>(1, Scheduler);
            SelectedItemObservable = _SelectedItemSubject.AsObservable();
        }

        private void correctSelectedIndex(IList<T> items, T selectedItem) {
            if (items != null) {
                if (items.Count == 0)
                    SelectedIndex = -1;
                else {
                    var selectedIdx = items.IndexOf(selectedItem);
                    if (selectedIdx != -1)
                        SelectedIndex = selectedIdx;
                    else
                        SelectedIndex = 0;
                }
            }
            else
                SelectedIndex = -1;
        }

        public IDisposable Subscribe(IObserver<T> observer) {
            return _SelectedItemSubject.Subscribe(observer);
        }
    }
}
