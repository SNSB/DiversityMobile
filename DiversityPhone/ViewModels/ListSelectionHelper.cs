using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System;

namespace DiversityPhone.ViewModels
{
    public class ListSelectionHelper<T> : ReactiveObject, IObserver<IList<T>>, IObservable<T>
    {        
        public IList<T> Items { get { return _Items.Value; } }
        public IObservable<IList<T>> ItemsObservable { get { return _ItemsSubject; } }
        private ObservableAsPropertyHelper<IList<T>> _Items;

        private int _SelectedIndex = -1;
        public int SelectedIndex
        {
            get
            {
                return _SelectedIndex;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SelectedIndex, ref _SelectedIndex, value);
            }
        }


        private ISubject<IList<T>> _ItemsSubject = new Subject<IList<T>>();
        private ISubject<T> _SelectedItemSubject = new ReplaySubject<T>(1);        

        private ObservableAsPropertyHelper<T> _SelectedItem;
        public T SelectedItem
        {
            get 
            {
                return _SelectedItem.Value;
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
                .Subscribe(_SelectedItemSubject);

            _SelectedItem = _SelectedItemSubject
                .ToProperty(this, x => x.SelectedItem, default(T));            

            _Items = _ItemsSubject
                .Do(items => correctSelectedIndex(items, SelectedItem))
                .ToProperty(this, x => x.Items);
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
            _ItemsSubject.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _SelectedItemSubject.Subscribe(observer);
        }
    }
}
