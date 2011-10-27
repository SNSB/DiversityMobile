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

namespace DiversityPhone.Services
{
    

    public class RotatingCache<T> : IList<T> where T : IEquatable<T>
    {
        /// <summary>
        /// Defines a function that can serve as a source for the cache
        /// </summary>
        /// <param name="count">Number Of Items to Retrieve</param>
        /// <param name="offset">Number Of Items To Skip</param>
        /// <returns>As many items as possible</returns>
        public delegate IEnumerable<T> CacheSource(int count, int offset);       

        private CacheSource _source;
        private T[] _store;
        private int _lowerBoundIdx;
        private int _lowerBoundKey;        
        private int _upperBoundKey;

        public RotatingCache(int size, CacheSource source)
        {
            this._source = source;
            this._store = new T[size];
            this._lowerBoundIdx = 0;
            this._lowerBoundKey = 0;            
            this._upperBoundKey = 0;
        }

        private T getItem(int idx)
        {
            if (!isCacheHit(idx))
                fetchRangeAround(idx);
            return _store[cacheOffset(_lowerBoundKey,idx)];
            
        }

        private void fetchRangeAround(int idx)
        {
            int lowerKey = idx - (_store.Length / 2);
            lowerKey = (lowerKey < 0) ? 0 : lowerKey;

            

    
        }

        private bool isCacheHit(int idx)
        {
            int itemCount = _upperBoundKey - _lowerBoundKey;
            int Offset = cacheOffset(_lowerBoundKey, idx);
            return (Offset > 0 && Offset < itemCount);
        }

        private int cacheIndex(int idx)
        {
            return 0;
        }

        private int cacheOffset(int baseIdx, int idx)
        {
            return idx - baseIdx;
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(T item)
        {
#if DEBUG
            throw new NotImplementedException();
#endif
        }

        public void Add(T item)
        {
#if DEBUG
            throw new NotImplementedException();
#endif
        }

        public void Clear()
        {
#if DEBUG
            throw new NotImplementedException();
#endif
        }

        public void Insert(int index, T item)
        {
#if DEBUG
            throw new NotImplementedException();
#endif
        }

        public void RemoveAt(int index)
        {
#if DEBUG
            throw new NotImplementedException();
#endif
        }
    }
}
