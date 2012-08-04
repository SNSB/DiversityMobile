using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using System.Device.Location;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Collections.Generic;
using DiversityPhone.Messages;



namespace DiversityPhone.Services
{
    public class GeoLocationService : IGeoLocationService, IDisposable
    {

        private IList<IDisposable> _subscriptions;

        private GeoCoordinateWatcher _Watcher;
        public GeoCoordinateWatcher Watcher
        {
            get { return _Watcher; }
        }

        #region Services

        private IMessageBus MessageBus;
        private ISettingsService SettingsService;

        #endregion

        #region defaults
        private static readonly TimeSpan DefaultStartupTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan DefaultLocationTimeout = TimeSpan.FromSeconds(20);
        private readonly GeoPositionAccuracy defaultAccuracy = GeoPositionAccuracy.High;
        private readonly int DefaultMovementTreshold = 20;
        #endregion

        #region TourSettings

 
        private int movementTreshhold;
        private int? CurrentSeriesID = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationService"/> class.
        /// </summary>
        public GeoLocationService(IMessageBus bus, ISettingsService settings)
            : this(GeoPositionAccuracy.Default, bus, settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationService"/> class with a default accuracy specified.
        /// <param name="defaultAccuracy">
        /// The default accuracy to be used for all requests for location information.
        /// </param>
        /// <param name="log">
        /// The logger used to capture diagnostics information.
        /// </param>
        /// </summary>
        public GeoLocationService(GeoPositionAccuracy defaultAccuracy, IMessageBus bus, ISettingsService settings)
        {
            MessageBus = bus;
            SettingsService = settings;
            this.defaultAccuracy = defaultAccuracy;
            movementTreshhold = DefaultMovementTreshold;
            _subscriptions = new List<IDisposable>()
            {
                 MessageBus.Listen<Model.EventSeries>(MessageContracts.START)
                    .Subscribe(es => setTourEventSeriesID(es.SeriesID.Value)),
                 MessageBus.Listen<Model.EventSeries>(MessageContracts.STOP)
                    .Subscribe(es => stopTour()),
            };
        }

        #endregion


        #region Georeferencing

        public bool IsWatching()
        {
            return !(_Watcher == null);
        }

        public void startWatcher()
        {
            _Watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            _Watcher.MovementThreshold = movementTreshhold;
            _Watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            _Watcher.Start();
        }

        public void stopWatcher()
        {
            if (_Watcher != null && _Watcher.Position!=null)
            {
                storeGeoPoint(_Watcher.Position.Location);
                _Watcher = null;
            }

        }

        public void setTourEventSeriesID(int SeriesID)
        {
            Model.AppSettings set = SettingsService.getSettings();
            set.CurrentSeriesID = SeriesID;
            SettingsService.saveSettings(set);
            CurrentSeriesID = SeriesID;
            if (_Watcher!=null && _Watcher.Position!=null)
            {
                storeGeoPoint(_Watcher.Position.Location);
            }
        }

        public void stopTour()
        {
            if (_Watcher != null && _Watcher.Position != null)
                storeGeoPoint(_Watcher.Position.Location);
            Model.AppSettings set = SettingsService.getSettings();
            set.CurrentSeriesID = null;
            SettingsService.saveSettings(set);
            CurrentSeriesID = null;  
        }

        private void storeGeoPoint(GeoCoordinate gc)
        {
            if (CurrentSeriesID == null)
                return;
            Model.GeoPointForSeries gp = new Model.GeoPointForSeries();
            gp.Altitude = gc.Altitude;
            gp.Latitude = gc.Latitude;
            gp.Longitude = gc.Longitude;
            gp.SeriesID = (int) CurrentSeriesID;
            MessageBus.SendMessage<Model.GeoPointForSeries>(gp, MessageContracts.SAVE);
           
        }

        public void fillGeoCoordinates(Model.ILocalizable loc)
        {
            if (_Watcher != null && _Watcher.Status == GeoPositionStatus.Ready)
            {
                var geoPos = _Watcher.Position;
                loc.Altitude = geoPos.Location.Altitude;
                loc.Latitude = geoPos.Location.Latitude;
                loc.Longitude = geoPos.Location.Longitude;

            }
            else
            {
                loc.Altitude = null;
                loc.Latitude = null;
                loc.Longitude = null;
            }
        }


        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (_Watcher == null)
                return;
            if (CurrentSeriesID != null)
            {
                storeGeoPoint(e.Position.Location);
            }
        }
       
        #endregion

        //#region StandardInterface
        ///// <summary>
        ///// The current status of location services for the device
        ///// </summary>
        ///// <returns>Returns the current status of locaton services for the device</returns>
        //public IObservable<GeoPositionStatus> Status()
        //{
        //    return this.StatusImpl()
        //        .Select(s => s);
        //}

        ///// <summary>
        ///// The current location.
        ///// </summary>
        ///// <returns>
        ///// Returns the current location.
        ///// </returns>
        //public IObservable<GeoCoordinate> Location()
        //{

        //    return this.LocationImpl(this.defaultAccuracy, DefaultLocationTimeout)
        //           .Select(l => l);
        //}

        ///// <summary>
        ///// The current location with the specified accuracy.
        ///// </summary>
        ///// <param name="accuracy">
        ///// The geo-location accuracy to be used.
        ///// </param>
        ///// <param name="locationTimeout">
        ///// The location timeout for the geo-location returned, every value has a timestamp and if the timestamp is greater than the location timeout then 
        ///// the value is ignored and we wait for the next value (should be instantieous.
        ///// </param>
        ///// <returns>
        ///// Returns the current location.
        ///// </returns>
        //public IObservable<GeoCoordinate> Location(GeoPositionAccuracy accuracy, TimeSpan locationTimeout)
        //{
        //    return this.LocationImpl(accuracy, locationTimeout)
        //        .Select(l => l);
        //}

        ///// <summary>
        ///// The location by time threshold, the location is returned dependent on the frequency and if the current
        ///// location has changed.
        ///// </summary>
        ///// <param name="frequency">
        ///// The frequency at which the current location will be published - frequency in milliseconds.
        ///// </param>
        ///// <returns>
        ///// Returns the current location.
        ///// </returns>
        //public IObservable<GeoCoordinate> LocationByTimeThreshold(int frequency)
        //{

        //    return this.LocationByTimeImpl(new TimeSpan(0, 0, 0, 0, frequency), this.defaultAccuracy)
        //            .Select(l => l);
        //}


        ///// <summary>
        ///// The location by distance threshold, the location is returned dependent on exceeding the distance.
        ///// </summary>
        ///// <param name="distance">
        ///// The distance that has to be exceeeded to trigger the current location being published - distance in metres.
        ///// </param>
        ///// <returns>
        ///// Returns the current location.
        ///// </returns>
        //public IObservable<GeoCoordinate> LocationByDistanceThreshold(int distance)
        //{
        //    return this.LocationByDistanceThresholdImpl(distance, this.defaultAccuracy)
        //        .Select(l => l);
        //}


        

        //#endregion

        //#region Impl

        //private IObservable<GeoPositionStatus> StatusImpl()
        //{
        //    var subject = new Subject<GeoPositionStatus>();

        //    try
        //    {
        //        var watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
        //        watcher.StatusChanged += (o, args) =>
        //        {
        //            subject.OnNext(args.Status);

        //            if (args.Status != GeoPositionStatus.Initializing &&
        //                args.Status != GeoPositionStatus.NoData)
        //            {
        //                subject.OnCompleted();
        //            }
        //        };

        //        Scheduler.ThreadPool.Schedule(() =>
        //        {
        //            var started = watcher.TryStart(true, DefaultStartupTimeout);
        //            if (!started)
        //            {
        //                subject.OnError(new Exception("Failed to start GeoCoordinateWatcher!"));
        //            }
        //        });

        //        return subject.AsObservable()
        //            .Finally(() =>
        //            {
        //                watcher.Stop();
        //            });
        //    }
        //    catch (Exception exn)
        //    {
        //        subject.OnError(exn);
        //        return subject.AsObservable();
        //    }
        //}

        //private IObservable<GeoCoordinate> LocationByDistanceThresholdImpl(int distance, GeoPositionAccuracy accuracy)
        //{
        //    var subject = new BehaviorSubject<GeoCoordinate>(GeoCoordinate.Unknown);

        //    try
        //    {
        //        var watcher = new GeoCoordinateWatcher(accuracy) { MovementThreshold = distance };
        //        watcher.PositionChanged += (o, args) =>
        //        {
        //            var newLocation = args.Position.Location;
        //            subject.OnNext(newLocation);
        //        };

        //        Scheduler.ThreadPool.Schedule(() =>
        //        {
        //            var started = watcher.TryStart(true, DefaultStartupTimeout);
        //            if (!started)
        //            {
        //                subject.OnError(new Exception("Failed to start GeoCoordinateWatcher!"));
        //            }
        //        });

        //        return subject.Where(c => !c.IsUnknown)
        //            .DistinctUntilChanged()
        //            .Finally(() =>
        //             {

        //                 watcher.Stop();
        //             })
        //            .AsObservable();
        //    }
        //    catch (Exception exn)
        //    {
        //        subject.OnError(exn);

        //        return subject.AsObservable();
        //    }
        //}

        //private GeoCoordinate CurrentLocationByTimeImpl(TimeSpan timeSpan, GeoPositionAccuracy accuracy)
        //{
        //    GeoCoordinateWatcher watcher = null;
        //    try
        //    {
        //        var currentLocation = GeoCoordinate.Unknown;
        //        watcher = new GeoCoordinateWatcher(accuracy);

        //        if (watcher.TryStart(true, timeSpan))
        //        {
        //            currentLocation = watcher.Position.Location;
        //        }
        //        else
        //        {

        //        }

        //        return currentLocation;
        //    }
        //    catch (Exception exn)
        //    {
        //        return GeoCoordinate.Unknown;
        //    }
        //    finally
        //    {
        //        if (watcher != null)
        //        {
        //            watcher.Stop();
        //        }
        //    }
        //}

        //private IObservable<GeoCoordinate> LocationImpl(GeoPositionAccuracy accuracy, TimeSpan locationTimeout)
        //{
        //    var subject = new Subject<GeoCoordinate>();

        //    try
        //    {
        //        var watcher = new GeoCoordinateWatcher(accuracy);
        //        watcher.PositionChanged += (o, args) =>
        //        {
        //            var newLocation = args.Position.Location;

        //            if ((DateTime.Now - args.Position.Timestamp.DateTime) < locationTimeout)
        //            {
        //                subject.OnNext(newLocation);
        //                subject.OnCompleted();
        //            }
        //        };

        //        Scheduler.ThreadPool.Schedule(() =>
        //        {
        //            var started = watcher.TryStart(true, DefaultStartupTimeout);
        //            if (!started)
        //            {
        //                subject.OnError(new Exception("Failed to start GeoCoordinateWatcher!"));
        //            }
        //        });

        //        return subject.AsObservable()
        //            .Where(c => !c.IsUnknown)
        //            .Finally(() =>
        //            {

        //                watcher.Stop();
        //            });
        //    }
        //    catch (Exception exn)
        //    {
        //        subject.OnError(exn);
        //        return subject.AsObservable();
        //    }
        //}

        //private IObservable<GeoCoordinate> LocationByTimeImpl(TimeSpan timeSpan, GeoPositionAccuracy accuracy)
        //{
        //    try
        //    {
        //        return Observable.Interval(timeSpan)
        //            .Select(t => this.CurrentLocationByTimeImpl(timeSpan, accuracy))
        //            .Where(c => !c.IsUnknown)
        //            .DistinctUntilChanged();
        //    }
        //    catch (Exception exn)
        //    {

        //        return Observable.Return(GeoCoordinate.Unknown);
        //    }
        //}

        //#endregion


        public void Dispose()
        {
            _Watcher.Dispose();
        }
    }
}
