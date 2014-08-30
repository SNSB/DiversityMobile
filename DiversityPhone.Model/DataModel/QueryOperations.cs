using System;
using System.Linq;
using System.Linq.Expressions;

namespace DiversityPhone.Model
{
    public class QueryOperations<T> : IQueryOperations<T>
    {
        private Func<IQueryable<T>, T, IQueryable<T>> keySmaller;
        private Func<IQueryable<T>, T, IQueryable<T>> keyEquals;
        private Func<IQueryable<T>, IQueryable<T>> orderbyKey;
        private Action<IQueryable<T>, T> setToFreeKey;

        public QueryOperations(Func<IQueryable<T>, T, IQueryable<T>> keySmaller,
            Func<IQueryable<T>, T, IQueryable<T>> keyEquals,
            Func<IQueryable<T>, IQueryable<T>> orderbyKey,
            Action<IQueryable<T>, T> setToFreeKey)
        {
            this.keySmaller = keySmaller;
            this.keyEquals = keyEquals;
            this.orderbyKey = orderbyKey;
            this.setToFreeKey = setToFreeKey;
        }

        public System.Linq.IQueryable<T> WhereKeySmallerThan(System.Linq.IQueryable<T> query, T item)
        {
            if (keySmaller != null)
                return keySmaller(query, item);
            else
                return query;
        }

        public System.Linq.IQueryable<T> WhereKeyEquals(System.Linq.IQueryable<T> query, T item)
        {
            if (keyEquals != null)
                return keyEquals(query, item);
            else
                return query;
        }

        public System.Linq.IQueryable<T> OrderbyKey(System.Linq.IQueryable<T> query, bool asc = true)
        {
            if (orderbyKey != null)
                return orderbyKey(query);
            else
                return query;
        }

        public void SetFreeKeyOnItem(System.Linq.IQueryable<T> table, T item)
        {
            if (setToFreeKey != null)
                setToFreeKey(table, item);
        }

        public static int FindFreeIntKey(IQueryable<T> table, Expression<Func<T, int>> keySelector)
        {
            var lowerthanMin = -1;

            if (table.Any())
                lowerthanMin = table.Select(keySelector).Min() - 1;

            return (lowerthanMin < -1) ? lowerthanMin : -1;
        }

        public static int FindFreeIntKeyUp(IQueryable<T> table, Expression<Func<T, int>> keySelector) //CollectionEventSeries Autoinckey is lowered by one by default in DiversityCollection. As we need to avoid Synchronisationconflicts we need to count in the other direction.
        {
            var largerThanMax = 1;

            if (table.Any())
                largerThanMax = table.Select(keySelector).Max() + 1;

            return (largerThanMax > 1) ? largerThanMax : 1;
        }
    }
}