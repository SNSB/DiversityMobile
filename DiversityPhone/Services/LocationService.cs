namespace DiversityPhone.Services
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using System;
    using System.Collections.Generic;
    using System.Device.Location;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;




    /// <summary>
    /// The location service, uses reactive extensions to publish the current location.
    /// </summary>
    public sealed class LocationService : ReactiveObject, ILocationService, IEnableLogger
    {
        private static readonly TimeSpan DefaultStartupTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan DefaultLocationTimeout = TimeSpan.FromSeconds(20);
        private static readonly GeoPositionAccuracy DefaultAccuracy = GeoPositionAccuracy.High;

        private IDisposable current_watcher = Disposable.Empty;

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

        private BehaviorSubject<GeoCoordinate> _LatestLocation = new BehaviorSubject<GeoCoordinate>(null);

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationService"/> class with default values for all parameters
        /// </summary>
        public LocationService()
        {
            this.WhenAny(x => x.IsEnabled, x => x.Value)
                .Where(enabled => enabled)
                .Select(_ => Observable.Create<GeoCoordinateWatcher>(obs =>
                {
                    var w = new GeoCoordinateWatcher(DefaultAccuracy);
                    obs.OnNext(w);
                    w.Start();

                    this.Log().Info("New GeoCoordinateWatcher Created.");

                    current_watcher = Disposable.Create(() => w.Stop());

                    return current_watcher;
                }))
            .Switch()
            .Select(w =>
                Observable.FromEventPattern<GeoPositionChangedEventArgs<GeoCoordinate>>(w, "PositionChanged")
                .Select(ev => ev.EventArgs.Position)
                .Select(pos => (pos != null) ? pos.Location : null)
            ).Switch()
            .Merge(
                // When we are disabled, clear out location information.
                this.WhenAny(x => x.IsEnabled, x => x.Value)
                .Where(enabled => !enabled)
                .Select(_ => null as GeoCoordinate)
                )
            .Subscribe(_LatestLocation);
        }

        public IObservable<Coordinate> LocationByDistanceThreshold(int distance)
        {
            var comparer = new DistanceThresholdComparer(distance);

            return _LatestLocation
                .DistinctUntilChanged(comparer)
                .ToCoordinates()
                .AsObservable();
        }

        public IObservable<Coordinate> Location()
        {
            return _LatestLocation
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