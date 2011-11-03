using System;
using System.Collections.Generic;

namespace DiversityPhone.Test
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

        private T getItem(int key)
        {
            if (!isCacheHit(key))
                fetchRangeAround(key);
            return _store[cacheIndex(key)];
            
        }

        private void fetchRangeAround(int idx)
        {
            int newlowerBoundKey = idx - (_store.Length / 2);
            newlowerBoundKey = (newlowerBoundKey < 0) ? 0 : newlowerBoundKey;
            int lowerKey = newlowerBoundKey;
            
            int itemCount = _store.Length;

            if (_lowerBoundKey > lowerKey && _lowerBoundKey - lowerKey < _store.Length)
                itemCount = _lowerBoundKey - lowerKey;
            else if (_upperBoundKey > lowerKey && _lowerBoundKey < lowerKey)
            {
                int overlap = _upperBoundKey - lowerKey;
                lowerKey = _upperBoundKey;
                itemCount -= overlap;
            }

            _upperBoundKey = lowerKey;
            int currentIdx = cacheIndex(lowerKey);
            
            foreach (var item in _source(itemCount, lowerKey))
            {
                _store[currentIdx] = item;
                currentIdx = ++currentIdx % _store.Length;
                _upperBoundKey++;
            }

            _lowerBoundIdx = cacheIndex(newlowerBoundKey);
            _lowerBoundKey = newlowerBoundKey;           
        }

        private bool isCacheHit(int key)
        {
            int itemCount = _upperBoundKey - _lowerBoundKey;
            int Offset = key - _lowerBoundKey;
            return (Offset > 0 && Offset < itemCount);
        }

        private int cacheIndex(int key)
        {
            return ((key - _lowerBoundKey)+_lowerBoundIdx) % _store.Length;
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get
            {
                return getItem(index);
            }
            set
            {
                
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
