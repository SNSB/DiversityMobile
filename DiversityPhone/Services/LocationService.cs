namespace DiversityPhone.Services
{
    using System;
    using System.Device.Location;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using ReactiveUI;
    using System.Reactive.Disposables;
    using System.Collections.Generic;

    using DiversityPhone.Model;
    using System.Threading;
    using DiversityPhone.Interface;


    public class Coordinate : ILocalizable
    {
        public static readonly Coordinate Unknown = new Coordinate();


        public double? Altitude
        {
            get;
            set;
        }

        public double? Latitude
        {
            get;
            set;
        }

        public double? Longitude
        {
            get;
            set;
        }
    }

    public static class CoordinateMixin
    {
        public static bool IsUnknown(this Coordinate This)
        {
            return !This.Latitude.HasValue && !This.Longitude.HasValue && !This.Altitude.HasValue;
        }
    }

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

    /// <summary>
    /// The location service, uses reactive extensions to publish the current location.
    /// </summary>
    public sealed class LocationService : ILocationService, IEnableLogger
    {
        private static readonly TimeSpan DefaultStartupTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan DefaultLocationTimeout = TimeSpan.FromSeconds(20);
        private static readonly GeoPositionAccuracy DefaultAccuracy = GeoPositionAccuracy.Default;

        readonly IScheduler threadpool = ThreadPoolScheduler.Instance;


        private IObservable<GeoCoordinateWatcher> watcher;
        private IObservable<GeoPosition<GeoCoordinate>> coordinate_observable;


        private GeoCoordinateWatcher _CurrentWatcher;
        private GeoCoordinateWatcher CurrentWatcher
        {
            get { return _CurrentWatcher; }
            set
            {
                if (_CurrentWatcher != value)
                {
                    if (_CurrentWatcher != null)
                        _CurrentWatcher.Dispose();
                    _CurrentWatcher = value;
                }
            }
        }

        private bool _IsEnabled = true;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;
                    lock (this)
                    {
                        var curr_w = CurrentWatcher;
                        if (curr_w != null)
                        {
                            if (!_IsEnabled)
                            {
                                curr_w.Stop();
                            }
                            else
                            {
                                curr_w.Start();
                            }
                        }
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
        public LocationService()
        {


            this.watcher = Observable.Defer(() => Observable.Start(() => new GeoCoordinateWatcher(DefaultAccuracy)))
                .Where(w => IsEnabled)
                .Do(w => CurrentWatcher = w)
                .Do(w => w.Start())
                .Replay(1)
                .RefCount();



            coordinate_observable = watcher
                .SelectMany(w => Observable.FromEventPattern<GeoPositionChangedEventArgs<GeoCoordinate>>(w, "PositionChanged")
                    .Select(ev => ev.EventArgs.Position)
                    );
        }

        public IObservable<Coordinate> LocationByDistanceThreshold(int distance)
        {
            var comparer = new DistanceThresholdComparer(distance);

            return coordinate_observable.Select(p => p.Location)
                .DistinctUntilChanged(comparer)
                .ToCoordinates()
                .AsObservable();
        }

        public IObservable<Coordinate> Location()
        {
            return coordinate_observable.Select(p => p.Location)
                 .ToCoordinates()
                 .AsObservable();
        }

        private class DistanceThresholdComparer : IEqualityComparer<GeoCoordinate>
        {
            double distance_threshold;

            public DistanceThresholdComparer(double threshold)
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