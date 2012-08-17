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
using Funq;
    using DiversityPhone.Model;

    /// <summary>
    /// The location service, uses reactive extensions to publish the current location.
    /// </summary>
    public sealed class LocationService : ILocationService, IEnableLogger
    {
        private static readonly TimeSpan DefaultStartupTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan DefaultLocationTimeout = TimeSpan.FromSeconds(20);
        private static readonly GeoPositionAccuracy DefaultAccuracy = GeoPositionAccuracy.Default;

        /// <summary>
        /// The logger used to capture diagnostics information.
        /// </summary>
        private readonly Logger log;

        IMessageBus Messenger;

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
                _IsEnabled = value;
                lock (this)
                {
                    if (!_IsEnabled && watcher_refcount != null)
                    {
                        watcher_refcount.Dispose();
                        watcher_refcount = null;
                    }
                }
            }
        }          

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationService"/> class with a default accuracy specified.
        /// <param name="defaultAccuracy">
        /// The default accuracy to be used for all requests for location information.
        /// </param>
        /// </summary>
        public LocationService(Container ioc) 
        {
            Messenger = ioc.Resolve<IMessageBus>();

            Messenger.Listen<AppSettings>()
                .Select(s => s.UseGPS)
                .Subscribe(gps => IsEnabled = gps);

            this.log = this.Log();
            this.watcher = new GeoCoordinateWatcher(DefaultAccuracy);

            this.watcher.PositionChanged += (s, args) => coordinate_subject.OnNext(args.Position);
            this.watcher.StatusChanged += (s, args) => status_subject.OnNext(args.Status);

        }

        public IObservable<GeoCoordinate> LocationByTimeThreshold(TimeSpan frequency)
        {
            return LocationByTimeThresholdImpl(frequency);
        }

        public IObservable<GeoCoordinate> LocationByDistanceThreshold(int distance)
        {
            return LocationByDistanceThresholdImpl(distance);
        }

        public IObservable<GeoPositionStatus> Status()
        {
            return StatusImpl();
        }

        public IObservable<GeoCoordinate> Location()
        {
            return LocationImpl();
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

        private IObservable<GeoCoordinate> LocationImpl()
        {
            var watcher_handle = StartWatcherIfNecessary();

            return coordinate_subject.Select(p => p.Location)                
                .Finally(() => watcher_handle.Dispose())
                .AsObservable();
        }

        private GeoCoordinate CurrentLocationWithTimeoutImpl(TimeSpan timeSpan)
        {
            var watcher_handle = StartWatcherIfNecessary();

            return coordinate_subject.Select(p => p.Location).Where(l => !l.IsUnknown)
                .Timeout(timeSpan, Observable.Return(GeoCoordinate.Unknown))
                .Finally(() => watcher_handle.Dispose())
                .First();
        }      

        private IObservable<GeoCoordinate> LocationByTimeThresholdImpl(TimeSpan timeSpan)
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
                    watcher_refcount = new RefCountDisposable(Disposable.Create(() =>
                        {
                            watcher.Stop();
                        }));

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