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
    using System.Threading;

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
        private IDisposable watcher_activation = Disposable.Empty;
        private int watcher_refcount = 0;
        private IObservable<GeoPositionStatus> status_observable;
        private IObservable<GeoPosition<GeoCoordinate>> coordinate_observable;

        private bool _IsEnabled = true;
        public bool IsEnabled 
        {
            get { return _IsEnabled; }
            set
            {
                _IsEnabled = value;
                lock (this)
                {
                    if (!_IsEnabled)
                    {
                        stopWatcher();
                    }
                    else
                    {
                        startWatcher();
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
                .Where(s => s != null)
                .Select(s => s.UseGPS)
                .Subscribe(gps => IsEnabled = gps);

            this.log = this.Log();
            this.watcher = new GeoCoordinateWatcher(DefaultAccuracy);

            

            coordinate_observable = Observable.FromEventPattern<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher, "PositionChanged").Select(ev => ev.EventArgs.Position);

            status_observable = Observable.FromEventPattern<GeoPositionStatusChangedEventArgs>(watcher, "StatusChanged").Select(ev => ev.EventArgs.Status);
        }

        public IObservable<Coordinate> LocationByTimeThreshold(TimeSpan frequency)
        {
            throw new NotImplementedException();
        }

        public IObservable<Coordinate> LocationByDistanceThreshold(int distance)
        {
            var comparer = new DistanceThresholdComparer(distance);

            return RefCount(coordinate_observable.Select(p => p.Location)
                .DistinctUntilChanged(comparer)
                .ToCoordinates()
                .AsObservable());                
        }

        public IObservable<GeoPositionStatus> Status()
        {
            return StatusImpl();
        }

        public IObservable<Coordinate> Location()
        {
            return RefCount(coordinate_observable.Select(p => p.Location)
                 .ToCoordinates()
                 .AsObservable());
        }       

        private IObservable<GeoPositionStatus> StatusImpl()
        {     
            var watcher_handle = StartWatcherIfNecessary();

            var valid_status_seen = false;

            return status_observable
                .TakeWhile(s => (valid_status_seen |= (s != GeoPositionStatus.Initializing && s != GeoPositionStatus.NoData)))
                .Finally(() => watcher_handle.Dispose())
                .AsObservable();               
        }

        private void startWatcher()
        {
            if(IsEnabled && watcher_refcount > 0)
                Scheduler.ThreadPool.Schedule(() =>
                {
                    var started = watcher.TryStart(true, DefaultStartupTimeout);
                });
        }

        private void stopWatcher()
        {
            watcher.Stop();
        }

        IDisposable StartWatcherIfNecessary()
        {            
            if (Interlocked.Increment(ref watcher_refcount) == 1)
            {
                startWatcher();
            }

            return Disposable.Create(() =>
                {
                    if (Interlocked.Decrement(ref watcher_refcount) == 0)
                        stopWatcher();
                });
        }

        private IObservable<T> RefCount<T>(IObservable<T> source)
        {
            return Observable.Create<T>((observer) =>
                {
                    var sub = source.Subscribe(observer);
                    var handle = StartWatcherIfNecessary();
                    return new CompositeDisposable(sub, handle) as IDisposable;
                });
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

    static class GeoCoordinateMixin
    {
        public static IObservable<Coordinate> ToCoordinates(this IObservable<GeoCoordinate> This)
        {
            return This.Select(g => new Coordinate()
            {
                Altitude = g.Altitude,
                Latitude = g.Latitude,
                Longitude = g.Longitude
            });
        }
    }
}