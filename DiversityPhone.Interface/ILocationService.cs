using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Interface
{   

    /// <summary>
    /// Interface defining the location service API.
    /// </summary>
    public interface ILocationService
    {
        /// <summary>
        /// The location by distance threshold, the location is returned dependent on exceeding the distance.
        /// </summary>
        /// <param name="distance">
        /// The distance that has to be exceeeded to trigger the current location being published - distance in metres.
        /// </param>
        /// <returns>
        /// Returns the current location.
        /// </returns>
        IObservable<Coordinate> LocationByDistanceThreshold(int distance);

        /// <summary>
        /// The current location.
        /// </summary>
        /// <returns>
        /// Returns the current location.
        /// </returns>
        IObservable<Coordinate> Location();

        bool IsEnabled { get; set; }
    }
}
