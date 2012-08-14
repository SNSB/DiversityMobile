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

        CompositeDisposable location_subscriptions = new CompositeDisposable();

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

             return this.LocationImpl(this.defaultAccuracy, locationTimeout)
                 .Select(l => l);
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

            return this.LocationByTimeImpl(frequency, this.defaultAccuracy)
                    .Select(l => l);
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

            return this.LocationByDistanceThresholdImpl(distance, this.defaultAccuracy)
                .Select(l => l);
        }

        private IObservable<GeoCoordinate> LocationByDistanceThresholdImpl(int distance, GeoPositionAccuracy accuracy)
        {
            var subject = new BehaviorSubject<GeoCoordinate>(GeoCoordinate.Unknown);

            try
            {
                var watcher = new GeoCoordinateWatcher(accuracy) { MovementThreshold = distance };
                watcher.PositionChanged += (o, args) =>
                {
                    var newLocation = args.Position.Location;
                    this.log.Debug("LocationService: New location(distance) - {0}, {1}", newLocation.Latitude, newLocation.Longitude);
                    subject.OnNext(newLocation);
                };

                Scheduler.ThreadPool.Schedule(() =>
                {
                    var started = watcher.TryStart(true, DefaultStartupTimeout);
                    if (!started)
                    {
                        subject.OnError(new Exception("Failed to start GeoCoordinateWatcher!"));
                    }
                });

                return subject.Where(c => !c.IsUnknown)
                    .DistinctUntilChanged()
                    .Finally(() =>
                     {
                        this.log.Debug("LocationService: Shutting down watcher...");

                        watcher.Stop();
                    })
                    .AsObservable();
            }
            catch (Exception exn)
            {
                this.log.Debug("LocationService: Failed LocationByDistanceThresholdImpl, message - '{0}'", exn.Message);
                subject.OnError(exn);

                return subject.AsObservable();
            }
        }

        private GeoCoordinate CurrentLocationByTimeImpl(TimeSpan timeSpan, GeoPositionAccuracy accuracy)
        {
            GeoCoordinateWatcher watcher = null;

            try
            {
                var currentLocation = GeoCoordinate.Unknown;
                watcher = new GeoCoordinateWatcher(accuracy);
                
                this.log.Debug("LocationService: Start time - " + DateTime.Now.ToUniversalTime());
                if (watcher.TryStart(true, timeSpan))
                {
                    currentLocation = watcher.Position.Location;
                    this.log.Debug("LocationService: Location calculated - " + DateTime.Now.ToUniversalTime());
                    this.log.Debug("LocationService: New location(time) - {0}, {1}", currentLocation.Latitude, currentLocation.Longitude);
                }
                else
                {
                    this.log.Debug("LocationService: Location NOT calculated");
                }
                
                return currentLocation;
            }
            catch (Exception exn)
            {
                this.log.Debug("LocationService: Failed CurrentLocationByTimeImpl, message - '{0}'", exn.Message);

                return GeoCoordinate.Unknown;
            }
            finally
            {
                if (watcher != null)
                {
                    watcher.Stop();
                }
            }
        }

        private IObservable<GeoCoordinate> LocationImpl(GeoPositionAccuracy accuracy, TimeSpan locationTimeout)
        {
            var subject = new Subject<GeoCoordinate>();

            try
            {
                var watcher = new GeoCoordinateWatcher(accuracy);
                watcher.PositionChanged += (o, args) =>
                {
                    var newLocation = args.Position.Location;
                    this.log.Debug("LocationService: New location(current) - {0}: {1}, {2}", args.Position.Timestamp, newLocation.Latitude, newLocation.Longitude);

                    if ((DateTime.Now - args.Position.Timestamp.DateTime) < locationTimeout)
                    {
                        subject.OnNext(newLocation);
                        subject.OnCompleted();
                    }
                };

                Scheduler.ThreadPool.Schedule(() =>
                {
                    var started = watcher.TryStart(true, DefaultStartupTimeout);
                    if (!started)
                    {
                        subject.OnError(new Exception("Failed to start GeoCoordinateWatcher!"));
                    }
                });

                return subject.AsObservable()
                    .Where(c => !c.IsUnknown)
                    .Finally(() =>
                    {
                        this.log.Debug("LocationService: Shutting down watcher...");
                    
                        watcher.Stop();
                    });
            }
            catch (Exception exn)
            {
                this.log.Debug("LocationService: Failed LocationImpl, message - '{0}'", exn.Message);
                subject.OnError(exn);

                return subject.AsObservable();
            }
        }

        private IObservable<GeoCoordinate> LocationByTimeImpl(TimeSpan timeSpan, GeoPositionAccuracy accuracy)
        {
            try
            {
                return Observable.Interval(timeSpan)
                    .Select(t => this.CurrentLocationByTimeImpl(timeSpan, accuracy))
                    .Where(c => !c.IsUnknown)
                    .Finally(() => this.log.Debug("LocationService: Shutting down watcher..."))
                    .DistinctUntilChanged();
            }
            catch (Exception exn)
            {
                this.log.Debug("LocationService: Failed LocationByTimeImpl, message - '{0}'", exn.Message);
                
                return Observable.Return(GeoCoordinate.Unknown);
            }
        }

        private IObservable<GeoPositionStatus> StatusImpl()
        {
            var subject = new Subject<GeoPositionStatus>();

            try
            {
                var watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
                watcher.StatusChanged += (o, args) =>
                {
                    this.log.Debug("LocationService: New Status - '{0}'", args.Status);
                    subject.OnNext(args.Status);

                    if (args.Status != GeoPositionStatus.Initializing &&
                        args.Status != GeoPositionStatus.NoData)
                    {
                        subject.OnCompleted();
                    }
                };

                Scheduler.ThreadPool.Schedule(() =>
                {
                    var started = watcher.TryStart(true, DefaultStartupTimeout);
                    if (!started)
                    {
                        subject.OnError(new Exception("Failed to start GeoCoordinateWatcher!"));
                    }
                });

                return subject.AsObservable()
                    .Finally(() =>
                    {
                        this.log.Debug("LocationService: Shutting down watcher...");

                        watcher.Stop();
                    });
            }
            catch (Exception exn)
            {
                this.log.Debug("LocationService: Failed StatusImpl, message - '{0}'", exn.Message);
                subject.OnError(exn);

                return subject.AsObservable();
            }
        }
    }
}