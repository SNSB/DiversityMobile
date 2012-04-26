using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Device.Location;

namespace DiversityPhone.Services
{
    public interface IGeoLocationService
    {

        bool IsWatching();

        /// <summary>
        /// Sets the Coordinates of an object implementing the ILocalizable interface to the current position of the watcher.
        /// </summary>
        /// <param name="loc">
        /// Object Implementing the ILocalizable interface (i.e. heaving Properies for altitude latitude and longitude).
        /// </param>
        void fillGeoCoordinates(Model.ILocalizable loc);

        void startWatcher();

        void stopWatcher();

        /// <summary>
        /// Starts collecting visited points and assoiciates these points with an EventSeries. There can only be one Tour active at a point of time.
        /// </summary>
        /// <param name="es">
        /// EventSeries for which the tour is created
       /// </param>
        void setTourEventSeriesID(int seriesID);

        /// <summary>
        /// Stops the actual tour. Hence, it is possible to start a new tour.
        /// </summary>
        /// </param>
        void stopTour();

        GeoCoordinateWatcher Watcher{get;}

        //// <summary>
        //// The location by time threshold, the location is returned dependent on the frequency and if the current
        //// location has changed.
        //// </summary>
        //// <param name="frequency">
        //// The frequency at which the current location will be published - frequency in milliseconds.
        //// </param>
        //// <returns>
        //// Returns the current location.
        //// </returns>
        ////IObservable<GeoCoordinate> LocationByTimeThreshold(int frequency);

       


        //// <summary>
        //// The location by distance threshold, the location is returned dependent on exceeding the distance.
        //// </summary>
        //// <param name="distance">
        //// The distance that has to be exceeeded to trigger the current location being published - distance in metres.
        //// </param>
        //// <returns>
        //// Returns the current location.
        //// </returns>
        ////IObservable<GeoCoordinate> LocationByDistanceThreshold(int distance);

     

        //// <summary>
        //// The current status of location services for the device
        //// </summary>
        //// <returns>Returns the current status of locaton services for the device</returns>
        ////IObservable<GeoPositionStatus> Status();

        //// <summary>
        //// The current location.
        //// </summary>
        //// <returns>
        //// Returns the current location.
        //// </returns>
        ////IObservable<GeoCoordinate> Location();



       
    }
}
