using System;
using System.Collections.Generic;


namespace DiversityPhone.Common
{
    public interface ICacheSource<T>
    {
        /// <summary>
        /// Defines a function that can serve as a source for the cache
        /// </summary>
        /// <param name="count">Number Of Items to Retrieve</param>
        /// <param name="offset">Number Of Items To Skip</param>
        /// <returns>As many items as possible</returns>
        IEnumerable<T> retrieveItems(int count, int offset);


        /// <summary>
        /// Searches the underlying Store for the given item.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>The 0 based Index of the item in the store, -1 if it is not found.</returns>
        int IndexOf(T item);        

        /// <summary>
        /// The number of elements in the underlying store.
        /// </summary>
        int Count { get; }
    }
}
