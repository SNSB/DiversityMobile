using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using ReactiveUI;

namespace DiversityPhone.ViewModels
{
    public class VirtualizingReadonlyViewModelList<T,VM> : IList<VM> //where T : ReactiveObject
    {
        private IList<T> _source;
        private Func<T, VM> _vmFactory;

        public VirtualizingReadonlyViewModelList(IList<T> source, Func<T,VM> viewModelFactory)
        {
            _source = source;
            _vmFactory = viewModelFactory;
        }

        public VM this[int index]
        {
            get
            {
                return _vmFactory(_source[index]);
            }
            set
            {
                
            }
        }

        public int Count
        {
            get { return _source.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public void Clear()
        {

        }

        public int IndexOf(VM item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, VM item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }        

        public void Add(VM item)
        {
            throw new NotImplementedException();
        }        

        public bool Contains(VM item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(VM[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }      

        public bool Remove(VM item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<VM> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
