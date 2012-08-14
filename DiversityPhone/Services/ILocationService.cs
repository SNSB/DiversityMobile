// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILocationService.cs" company="XamlNinja">
//   2011 Richard Griffin and Ollie Riches
// </copyright>
// <summary>
//   Interface defining the location service API.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DiversityPhone.Services
{
    using System;
    using System.Device.Location;

    /// <summary>
    /// Interface defining the location service API.
    /// </summary>
    public interface ILocationService
    {
        /// <summary>
        /// The location by time threshold, the location is returned dependent on the frequency and if the current
        /// location has changed.
        /// </summary>
        /// <param name="frequency">
        /// The frequency at which the current location will be published - frequency in milliseconds.
        /// </param>
        /// <returns>
        /// Returns the current location.
        /// </returns>
        IObservable<GeoCoordinate> LocationByTimeThreshold(int frequency);

        /// <summary>
        /// The location by time threshold, the location is returned dependent on the frequency and if the current
        /// location has changed.
        /// </summary>
        /// <param name="frequency">
        /// The frequency at which the current location will be published - frequency in milliseconds.
        /// </param>
        /// <param name="accuracy">
        /// The accuracy required for the location information.
        /// </param>
        /// <returns>
        /// Returns the current location.
        /// </returns>
        IObservable<GeoCoordinate> LocationByTimeThreshold(int frequency, GeoPositionAccuracy accuracy);

        /// <summary>
        /// The location by time threshold, the location is returned dependent on the frequency and if the current
        /// location has changed.
        /// </summary>
        /// <param name="frequency">
        /// The frequency at which the current location will be published - timespan value.
        /// </param>
        /// <returns>
        /// Returns the current location.
        /// </returns>
        IObservable<GeoCoordinate> LocationByTimeThreshold(TimeSpan frequency);

        /// <summary>
        /// The location by time threshold, the location is returned dependent on the frequency and if the current
        /// location has changed.
        /// </summary>
        /// <param name="frequency">
        /// The frequency at which the current location will be published - timespan value.
        /// </param>
        /// <param name="accuracy">
        /// The accuracy required for the location information.
        /// </param>
        /// <returns>
        /// Returns the current location.
        /// </returns>
        IObservable<GeoCoordinate> LocationByTimeThreshold(TimeSpan frequency, GeoPositionAccuracy accuracy);

        /// <summary>
        /// The location by distance threshold, the location is returned dependent on exceeding the distance.
        /// </summary>
        /// <param name="distance">
        /// The distance that has to be exceeeded to trigger the current location being published - distance in metres.
        /// </param>
        /// <returns>
        /// Returns the current location.
        /// </returns>
        IObservable<GeoCoordinate> LocationByDistanceThreshold(int distance);

        /// <summary>
        /// The location by distance threshold with specified accuracy, the location is returned dependent on exceeding the distance.
        /// </summary>
        /// <param name="distance">
        /// The distance that has to be exceeeded to trigger the current location being published - distance in metres.
        /// </param>
        /// <param name="accuracy">
        /// The accuracy required for the location information.
        /// </param>
        /// <returns>
        /// Returns the current location.
        /// </returns>
        IObservable<GeoCoordinate> LocationByDistanceThreshold(int distance, GeoPositionAccuracy accuracy);

        /// <summary>
        /// The current status of location services for the device
        /// </summary>
        /// <returns>Returns the current status of locaton services for the device</returns>
        IObservable<GeoPositionStatus> Status();

        /// <summary>
        /// The current location.
        /// </summary>
        /// <returns>
        /// Returns the current location.
        /// </returns>
        IObservable<GeoCoordinate> Location();

        /// <summary>
        /// The current location  
        /// </summary>
        /// <param name="accuracy">
        /// The accuracy required for the location information.
        /// </param>
        /// <returns>
        /// Returns the current location.
        /// </returns>
        IObservable<GeoCoordinate> Location(GeoPositionAccuracy accuracy);
        
        /// <summary>
        /// The current location.
        /// </summary>
        /// <param name="locationTimeout">
        /// The location timeout for the geo-location returned, every value has a timestamp and if the timestamp is greater than the location timeout then 
        /// the value is ignored and we wait for the next value (should be instantieous.
        /// </param>
        /// <returns>
        /// Returns the current location.
        /// </returns>
        IObservable<GeoCoordinate> Location(TimeSpan locationTimeout);

        /// <summary>
        /// The current location with the specified accuracy.
        /// </summary>
        /// <param name="accuracy">
        /// The geo-location accuracy to be used.
        /// </param>
        /// <param name="locationTimeout">
        /// The location timeout for the geo-location returned, every value has a timestamp and if the timestamp is greater than the location timeout then 
        /// the value is ignored and we wait for the next value (should be instantieous.
        /// </param>
        /// <returns>
        /// Returns the current location.
        /// </returns>
        IObservable<GeoCoordinate> Location(GeoPositionAccuracy accuracy, TimeSpan locationTimeout);
    }
}
