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

        int Count { get; }
    }
}
