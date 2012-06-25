using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Linq;
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
                {
                    _SelectedIndex = value;
                    this.RaisePropertyChanged(x => x.SelectedIndex);
                }
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
            if (items != null)
            {                
                if (items.Count == 0)
                    SelectedIndex = -1;
                else
                {
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
            var emptySelection = (value != null && !value.Any()) || (value == null);

            try
            {
                Items = value;
            }
            catch (InvalidOperationException)
            {
                if (!emptySelection)
                    throw;
            }

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
