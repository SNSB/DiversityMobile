using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System;

namespace DiversityPhone.ViewModels
{
    public class ListSelectionHelper<T> : ReactiveObject, IObserver<IList<T>>, IObservable<T>
    {
        private int deferredIndex = -1;
        private bool _UpdatingItems;
        private bool UpdatingItems 
        {
            get
            {
                return _UpdatingItems;
            }
            set
            {
                _UpdatingItems = value;
                if (!_UpdatingItems)
                    if (SelectedIndex != deferredIndex)
                        SelectedIndex = deferredIndex;
                if (_UpdatingItems)
                    deferredIndex = SelectedIndex;
            }
        }

        private IList<T> _Items = null;
        public IList<T> Items
        {
            get
            {
                return _Items;
            }
            private set
            {                
                this.RaiseAndSetIfChanged(x => x.Items, ref _Items, value);
            }
        }
        public IObservable<IList<T>> ItemsObservable { get { return _ItemsSubject; } }
        

        private int _SelectedIndex = -1;
        public int SelectedIndex
        {
            get
            {
                return _SelectedIndex;
            }
            set
            {
                if (UpdatingItems)
                    deferredIndex = value;
                else
                    this.RaiseAndSetIfChanged(x => x.SelectedIndex, ref _SelectedIndex, value);
            }
        }


        private ISubject<IList<T>> _ItemsSubject = new Subject<IList<T>>();
        private ISubject<T> _SelectedItemSubject = new ReplaySubject<T>(1);        

        private T _SelectedItem;
        public T SelectedItem
        {
            get 
            {
                return _SelectedItem;
            }
            set
            {
                correctSelectedIndex(Items, value);
            }
        }

        public ListSelectionHelper()
        {
            _ItemsSubject = new Subject<IList<T>>();
            this.ObservableForProperty(x => x.SelectedIndex)
                .Value()
                .Select(idx => (idx > -1) ? Items[idx] : default(T))
                .DistinctUntilChanged()
                .Do(val => _SelectedItem = val)
                .Subscribe(item => _SelectedItemSubject.OnNext(item));            
        }

        private void correctSelectedIndex(IList<T> items, T selectedItem)
        {
            var selectedIdx = (items != null) ? items.IndexOf(selectedItem) : -1;
            if (selectedIdx > -1)
                SelectedIndex = selectedIdx;

        }

        public void OnCompleted()
        {
            _ItemsSubject.OnCompleted();
        }

        public void OnError(Exception exception)
        {
            _ItemsSubject.OnError(exception);
        }

        public void OnNext(IList<T> value)
        {
            UpdatingItems = true;
            Items = value;
            correctSelectedIndex(value, SelectedItem);
            _ItemsSubject.OnNext(value);
            UpdatingItems = false;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _SelectedItemSubject.Subscribe(observer);
        }
    }
}
