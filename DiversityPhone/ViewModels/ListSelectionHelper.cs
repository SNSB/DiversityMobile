using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System;

namespace DiversityPhone.ViewModels
{
    public class ListSelectionHelper<T> : ReactiveObject
    {        
        public IList<T> Items { get { return _Items.Value; } }
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

        public ISubject<IList<T>> ItemsSubject { get; private set; }
        private ISubject<T> _SelectedItemSubject = new ReplaySubject<T>(1);
        public IObservable<T> SelectedItemObservable { get { return _SelectedItemSubject; } }

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
            ItemsSubject = new Subject<IList<T>>();
            this.ObservableForProperty(x => x.SelectedIndex)
                .Value()
                .Select(idx => (idx > -1) ? Items[idx] : default(T))
                .DistinctUntilChanged()
                .Subscribe(_SelectedItemSubject);

            _SelectedItem = _SelectedItemSubject
                .ToProperty(this, x => x.SelectedItem);

            _SelectedItemSubject.OnNext(default(T));

            _Items = ItemsSubject
                .Do(items => correctSelectedIndex(items, SelectedItem))
                .ToProperty(this, x => x.Items);
        }

        private void correctSelectedIndex(IList<T> items, T selectedItem)
        {
            var selectedIdx = (items != null) ? items.IndexOf(selectedItem) : -1;
            if (selectedIdx > -1)
                SelectedIndex = selectedIdx;

        }
    }
}
