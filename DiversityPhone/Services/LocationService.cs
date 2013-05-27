namespace DiversityPhone.Services
{
    using System;
    using System.Device.Location;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using ReactiveUI;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using DiversityPhone.Interface;
    using System.Reactive.Disposables;




    /// <summary>
    /// The location service, uses reactive extensions to publish the current location.
    /// </summary>
    public sealed class LocationService : ReactiveObject, ILocationService, IEnableLogger
    {
        private static readonly TimeSpan DefaultStartupTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan DefaultLocationTimeout = TimeSpan.FromSeconds(20);
        private static readonly GeoPositionAccuracy DefaultAccuracy = GeoPositionAccuracy.High;

        readonly IScheduler threadpool = ThreadPoolScheduler.Instance;


        private IDisposable current_watcher = Disposable.Empty;
        private IObservable<GeoPosition<GeoCoordinate>> coordinate_observable;

        private bool _IsEnabled = true;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;
                    if (!_IsEnabled)
                        current_watcher.Dispose();
                    this.RaisePropertyChanged(x => x.IsEnabled);
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


            var watcher = this.WhenAny(x => x.IsEnabled, x => x.Value)
                    .Where(enabled => enabled)
                    .Select(_ => Observable.Create<GeoCoordinateWatcher>(obs =>
                    {
                        var w = new GeoCoordinateWatcher(DefaultAccuracy);
                        obs.OnNext(w);
                        w.Start();

                        current_watcher = Disposable.Create(() => w.Stop());

                        return current_watcher;
                    }))
                .Switch()
                .Replay(1)
                .RefCount();



            coordinate_observable = watcher
                .Select(w => Observable.FromEventPattern<GeoPositionChangedEventArgs<GeoCoordinate>>(w, "PositionChanged")
                    .Select(ev => ev.EventArgs.Position)
                    ).Switch();
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

        IObservable<bool> ILocationService.IsEnabled()
        {
            return this.WhenAny(x => x.IsEnabled, x => x.Value);
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