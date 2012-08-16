// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocationService.cs" company="XamlNinja">
//   2011 Richard Griffin and Ollie Riches
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DiversityPhone.Services
{
    using System;
    using System.Device.Location;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using ReactiveUI;
    using NLog;
using System.Reactive.Disposables;
using System.Collections.Generic;

    /// <summary>
    /// The location service, uses reactive extensions to publish the current location.
    /// </summary>
    public sealed class LocationService : ILocationService, IEnableLogger
    {
        private static readonly TimeSpan DefaultStartupTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan DefaultLocationTimeout = TimeSpan.FromSeconds(20);

        /// <summary>
        /// The logger used to capture diagnostics information.
        /// </summary>
        private readonly Logger log;

        // Defines the default accuracy to be used.
        private readonly GeoPositionAccuracy defaultAccuracy = GeoPositionAccuracy.Default;

        private GeoCoordinateWatcher watcher = null;
        private RefCountDisposable watcher_refcount = null;
        private ISubject<GeoPositionStatus> status_subject = new BehaviorSubject<GeoPositionStatus>(GeoPositionStatus.Disabled);
        private ISubject<GeoPosition<GeoCoordinate>> coordinate_subject = new BehaviorSubject<GeoPosition<GeoCoordinate>>(new GeoPosition<GeoCoordinate>(DateTimeOffset.Now, GeoCoordinate.Unknown));

        private bool _IsEnabled = true;
        public bool IsEnabled 
        {
            get { return _IsEnabled; }
            set
            {
                
            }
        }          

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationService"/> class with a default accuracy specified.
        /// <param name="defaultAccuracy">
        /// The default accuracy to be used for all requests for location information.
        /// </param>
        /// </summary>
        public LocationService() 
        {            
            this.log = this.Log();
            this.watcher = new GeoCoordinateWatcher(defaultAccuracy);

            this.watcher.PositionChanged += (s, args) => coordinate_subject.OnNext(args.Position);
            this.watcher.StatusChanged += (s, args) => status_subject.OnNext(args.Status);
        }

        /// <summary>
        /// The current status of location services for the device
        /// </summary>
        /// <returns>Returns the current status of locaton services for the device</returns>
        public IObservable<GeoPositionStatus> Status()
        {
            return this.StatusImpl()
                .Select(s => s);
        }

        /// <summary>
        /// The current location.
        /// </summary>
        /// <returns>
        /// Returns the current location.
        /// </returns>
        public IObservable<GeoCoordinate> Location()
        {
            this.log.Debug("LocationService: Location...");
            this.log.Debug("LocationService: Accuracy (default) - {0} seconds", this.defaultAccuracy);
            this.log.Debug("LocationService: Timeout (default) - {0} seconds", DefaultLocationTimeout.TotalSeconds);
            
            return this.LocationImpl(this.defaultAccuracy, DefaultLocationTimeout)
                   .Select(l => l);
        }

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
        public IObservable<GeoCoordinate> Location(TimeSpan locationTimeout)
        {
            this.log.Debug("LocationService: Location...");
            this.log.Debug("LocationService: Accuracy (default) - {0} seconds", this.defaultAccuracy);
            this.log.Debug("LocationService: Timeout - {0} seconds", locationTimeout.TotalSeconds);

            return this.CurrentLocationByTimeImpl(locationTimeout);
        }

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
        public IObservable<GeoCoordinate> LocationByTimeThreshold(TimeSpan frequency)
        {
            this.log.Debug("LocationService: LocationByTimeThreshold...");
            this.log.Debug("LocationService: Frequency - '{0}'", frequency);

            return this.LocationByTimeImpl(frequency);
        }

        /// <summary>
        /// The location by distance threshold, the location is returned dependent on exceeding the distance.
        /// </summary>
        /// <param name="distance">
        /// The distance that has to be exceeeded to trigger the current location being published - distance in metres.
        /// </param>
        /// <returns>
        /// Returns the current location.
        /// </returns>
        public IObservable<GeoCoordinate> LocationByDistanceThreshold(int distance)
        {
            this.log.Debug("LocationService: LocationByDistanceThreshold...");
            this.log.Debug("LocationService: Distance - '{0}'", distance);

            return this.LocationByDistanceThresholdImpl(distance);                
        }

        private IObservable<GeoCoordinate> LocationByDistanceThresholdImpl(int distance)
        {
            var watcher_handle = StartWatcherIfNecessary();
            var comparer = new DistanceThresholdComparer(distance);

            return coordinate_subject
                .Select(p => p.Location)
                .DistinctUntilChanged(comparer)
                .Finally(() => watcher_handle.Dispose())
                .AsObservable();
        }

        private GeoCoordinate CurrentLocationByTimeImpl(TimeSpan timeSpan)
        {
            var watcher_handle = StartWatcherIfNecessary();

            return coordinate_subject.Select(p => p.Location).Where(l => !l.IsUnknown)
                .Timeout(timeSpan, Observable.Return(GeoCoordinate.Unknown))
                .Finally(() => watcher_handle.Dispose())
                .First();
        }      

        private IObservable<GeoCoordinate> LocationByTimeImpl(TimeSpan timeSpan)
        {
            var watcher_handle = StartWatcherIfNecessary();

            return Observable.Throttle(coordinate_subject.Select(p => p.Location), timeSpan)
                .Where(c => !c.IsUnknown)
                .Finally(() => watcher_handle.Dispose())
                .DistinctUntilChanged();
        }   

        private IObservable<GeoPositionStatus> StatusImpl()
        {     
            var watcher_handle = StartWatcherIfNecessary();

            var valid_status_seen = false;

            return status_subject
                .TakeWhile(s => (valid_status_seen |= (s != GeoPositionStatus.Initializing && s != GeoPositionStatus.NoData)))
                .Finally(() => watcher_handle.Dispose())
                .AsObservable();               
        }

        IDisposable StartWatcherIfNecessary()
        {
            lock (this)
            {
                if (watcher_refcount == null)
                {
                    watcher_refcount = new RefCountDisposable(Disposable.Create(() => watcher.Stop()));

                    Scheduler.ThreadPool.Schedule(() =>
                    {
                        var started = watcher.TryStart(true, DefaultStartupTimeout);
                        if (!started)
                        {
                            coordinate_subject.OnError(new Exception("Failed to start GeoCoordinateWatcher!"));
                            status_subject.OnError(new Exception("Failed to start GeoCoordinateWatcher!"));
                        }
                    });
                }

                return watcher_refcount.GetDisposable();
            }
        }

        private class DistanceThresholdComparer : IEqualityComparer<GeoCoordinate>
        {
            double distance_threshold;

            public DistanceThresholdComparer( double threshold )
            {
                if (threshold < 0.0)
                    throw new ArgumentOutOfRangeException("threshold");

                distance_threshold = threshold;
            }

            public bool Equals(GeoCoordinate x, GeoCoordinate y)
            {
                return x.GetDistanceTo(y) < distance_threshold;
            }

            public int GetHashCode(GeoCoordinate obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}