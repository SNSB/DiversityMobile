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
using Wintellect.Sterling;
using System.Linq;
using System.Reactive.Subjects;
using System.Collections.Generic;
using DiversityService.Model;
using System.Data.Linq;

namespace DiversityPhone.Services
{
    public class OfflineStorage : IOfflineStorage
    {
        public OfflineStorage()
        {
            var context = new DiversityDataContext();
            if (!context.DatabaseExists())
                context.CreateDatabase();

            
        }

        public void addEventSeries(global::DiversityService.Model.EventSeries newSeries)
        {            
            if (newSeries.IsModified != null || newSeries.SeriesID != default(int))
                throw new InvalidOperationException("Series is not new!");

            var ctx = new DiversityDataContext();
            newSeries.SeriesID = findFreeEventSeriesID(ctx);
            ctx.EventSeries.InsertOnSubmit(newSeries);
            ctx.SubmitChanges();
        }

       

        IQueryable<EventSeries> IOfflineFieldData.EventSeries
        {
            get { throw new NotImplementedException(); }
        }

        public IList<EventSeries> getAllEventSeries()
        {
            var ctx = new DiversityDataContext();
            return new LightListImpl<EventSeries>(ctx.EventSeries);
        }

        private int findFreeEventSeriesID(DiversityDataContext ctx)
        {
            int min = -1;
            if(ctx.EventSeries.Any())
               min = (from es in ctx.EventSeries select es.SeriesID).Min();
            return (min > -1)? -1 : min - 1;
        }

        private class LightListImpl<T> : IList<T>
        {
            IQueryable<T> _source;
            public LightListImpl(IQueryable<T> source)
            {
                _source = source;
            }

            public int Count
            {
                get { return _source.Count(); }
            }


            public T this[int index]
            {
                get
                {
                    return _source.Skip(index).First() ;
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public int IndexOf(T item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, T item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }            

            public void Add(T item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(T item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool Remove(T item)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _source.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _source.GetEnumerator();
            }
        }


        public IList<EventSeries> getEventSeriesByDescription(string query)
        {
            throw new NotImplementedException();
        }

        public IList<EventSeries> getNewEventSeries()
        {
            throw new NotImplementedException();
        }

        public EventSeries getEventSeriesByID(int id)
        {
            throw new NotImplementedException();
        }
    }
}
