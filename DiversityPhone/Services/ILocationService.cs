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
        /// The frequency at which the current location will be published - timespan value.
        /// </param>       
        /// <returns>
        /// Returns the current location.
        /// </returns>
        IObservable<GeoCoordinate> LocationByTimeThreshold(TimeSpan frequency);

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
    }
}
