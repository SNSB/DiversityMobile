using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data.Linq;

namespace DiversityPhone.Model
{
    public interface IQueryOperations<T>
    {
        IQueryable<T> WhereKeySmallerThan(IQueryable<T> query, T item);
        IQueryable<T> WhereKeyEquals(IQueryable<T> query, T item);
        IQueryable<T> OrderbyKey(IQueryable<T> query, bool asc = true);

        /// <summary>
        /// Sets Key Values on the given item, that do NOT appear in the given query.
        /// </summary>
        /// <param name="table">existing rows</param>
        /// <param name="item">row to be inserted</param>
        void SetFreeKeyOnItem(IQueryable<T> table, T item);
    }
}
