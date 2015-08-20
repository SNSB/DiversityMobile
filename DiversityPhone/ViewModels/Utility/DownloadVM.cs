namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public class DownloadVM : PageVMBase
    {
        public class SearchResult
        {
            public SearchResult(EventSeries series)
            {
                Series = new EventSeriesVM(series);
                Events = new ObservableCollection<EventVM>();
            }

            public EventSeriesVM Series { get; private set; }

            public ObservableCollection<EventVM> Events { get; private set; }

            public void Add(Event ev)
            {
                Events.
                    Add(new EventVM(ev));
            }
        }

        private const int MAX_RESULTS = 50;

        private readonly IDiversityServiceClient Service;
        private readonly IConnectivityService Connectivity;
        private readonly IFieldDataService Storage;
        private readonly IKeyMappingService Mappings;
        private readonly EventHierarchyLoader HierarchyLoader;

        public bool IsDownloading { get { return _IsDownloading.Value; } }

        private ObservableAsPropertyHelper<bool> _IsDownloading;

        public bool IsOnlineAvailable { get { return _IsOnlineAvailable.Value; } }

        private ObservableAsPropertyHelper<bool> _HaveMaxResults;

        public bool HaveMaxResults { get { return _HaveMaxResults.Value; } }

        private ObservableAsPropertyHelper<bool> _IsOnlineAvailable;

        public int ElementsDownloaded { get { return _ElementsDownloaded.Value; } }

        private ISubject<int> _ElementsDownloadedSubject;
        private ObservableAsPropertyHelper<int> _ElementsDownloaded;

        public ReactiveAsyncCommand SearchEvents { get; private set; }

        public ReactiveCollection<SearchResult> QueryResult { get; private set; }
        private ISubject<int> ResultCount = new Subject<int>();

        public ReactiveAsyncCommand DownloadElement { get; private set; }

        public ReactiveCommand CancelDownload { get; private set; }

        public DownloadVM(
            IDiversityServiceClient Service,
            IConnectivityService Connectivity,
            IFieldDataService Storage,
            IKeyMappingService Mappings,
            EventHierarchyLoader HierarchyLoader,
            [Dispatcher] IScheduler Dispatcher
            )
        {
            this.Service = Service;
            this.Connectivity = Connectivity;
            this.Storage = Storage;
            this.Mappings = Mappings;
            this.HierarchyLoader = HierarchyLoader;

            QueryResult = new ReactiveCollection<SearchResult>();

            _IsOnlineAvailable = Connectivity.Status().Select(s => s == ConnectionStatus.Wifi).Do(_ => { this.GetType(); }, ex => { }, () => { })
                .ToProperty(this, x => x.IsOnlineAvailable);

            _HaveMaxResults = ResultCount
                .Select(c => c >= MAX_RESULTS)
                .ToProperty(this, x => x.HaveMaxResults, false);

            SearchEvents = new ReactiveAsyncCommand(this.WhenAny(x => x.IsOnlineAvailable, x => x.Value));
            SearchEvents.ShowInFlightNotification(Notifications, DiversityResources.Download_SearchingEvents);
            SearchEvents.ThrownExceptions
                    .ShowServiceErrorNotifications(Notifications)
                    .ShowErrorNotifications(Notifications)
                    .Subscribe();
            SearchEvents
                .RegisterAsyncObservable(StartSearch)
                .TakeUntil(this.OnDeactivation())
                .Subscribe(QueryResult.Add);

            CancelDownload = new ReactiveCommand();

            DownloadElement = new ReactiveAsyncCommand(this.WhenAny(x => x.IsOnlineAvailable, x => x.Value));
            DownloadElement.ThrownExceptions
                .ShowServiceErrorNotifications(Notifications)
                .ShowErrorNotifications(Notifications)
                .Subscribe();
            DownloadElement
                .RegisterAsyncObservable(x => StartDownload(x)
                    .TakeUntil(CancelDownload)
                    .Scan(0, (acc, _) => ++acc)
                    .Do(_ElementsDownloadedSubject.OnNext)
                );

            _IsDownloading = DownloadElement.ItemsInflight
                .Select(x => x > 0)
                .ToProperty(this, x => x.IsDownloading);

            this.OnDeactivation()
                .Subscribe(_ => Messenger.SendMessage(EventMessage.Default, MessageContracts.INIT));

            _ElementsDownloadedSubject = new Subject<int>();
            _ElementsDownloaded = _ElementsDownloadedSubject.ToProperty(this, x => x.ElementsDownloaded, 0, Dispatcher);
        }

        private IObservable<SearchResult> StartSearch(object query)
        {
            QueryResult.Clear();

            var queryString = query as string ?? string.Empty;

            var seriesObs = Service.GetEventSeriesByQuery(queryString).Publish();
            var seriesResults = seriesObs
                .SelectMany(x => x) // Flatten
                .Select(es => new SearchResult(es))
                .Publish();

            var eventsObs = Service.GetEventsByLocality(queryString).Publish();
            var eventResults = eventsObs
                // Create a dictionary from the series that were returned from the query above
                .Zip(seriesObs.Select(s => s.ToDictionary(x => x.SeriesID)), (evs, dict) => new { Events = evs, Map = dict })
                .SelectMany(x => 
                    from ev in x.Events
                    // SeriesID at this point contains the CollectionSeriesID
                    // Only Download Series that we don't have already
                    where ev.SeriesID.HasValue && !x.Map.ContainsKey(ev.SeriesID.Value)
                    select ev.SeriesID.Value)
                .Distinct()
                .SelectMany(id => Service.GetEventSeriesByID(id))
                .Select(es => new SearchResult(es))
                .Publish();

            // Forward the maximum result count to show the result truncation hint if necessary
            Observable.Zip(
                seriesObs.Select(x => x.Count()),
                eventsObs.Select(x => x.Count()),
                (s, e) => Math.Max(s, e)
                    )
                    .CatchEmpty()
                    .Subscribe(ResultCount);

            var noSeriesResult = new SearchResult(NoEventSeriesMixin.NoEventSeries);

            var results =
                Observable.Concat(
                seriesResults,
                eventResults);

            eventsObs
                .Zip(results.ToDictionary(x => x.Series.Model.CollectionSeriesID), (evs, dict) => new { Events = evs, Map = dict })
                .CatchEmpty()
                .Subscribe(x =>
                {
                    var dict = x.Map;
                    foreach (var ev in x.Events)
                    {
                        // Dictionary cannot hold an item with a null key
                        if (!ev.SeriesID.HasValue)
                        {
                            noSeriesResult.Add(ev);
                        }
                        else if (dict.ContainsKey(ev.SeriesID))
                        {
                            var res = dict[ev.SeriesID];
                            res.Add(ev);
                        }
                        else
                        {
                            Debugger.Break();
                        }
                    }
                }); // ... for the side effects

            var searchResults = Observable.Merge(
                results,
                eventsObs
                // If we have an event from the NoEventSeries...
                .Where(evs => evs.Any(x => !x.SeriesID.HasValue))
                // ... add the result for it
                .Select(_ => noSeriesResult)
                ).Replay();

            searchResults.Connect();
            eventResults.Connect();
            seriesResults.Connect();
            eventsObs.Connect();
            seriesObs.Connect();

            return searchResults;
        }

        private IObservable<Unit> StartDownload(object root)
        {
            var resultObs = Observable.Empty<Unit>();
            if (root is EventSeriesVM) {
                var esvm = root as EventSeriesVM;
                var es = esvm.Model;

                if (NoEventSeriesMixin.IsNoEventSeries(es))
                {
                    Notifications.showNotification(DiversityResources.Download_CannotDownloadNoES);
                }
                else
                {
                    resultObs = HierarchyLoader.downloadAndStoreDependencies(es);
                }
            }
            else if(root is EventVM)
            {
                var vm = root as EventVM;

                resultObs = IfNotDownloadedYet(vm.Model)
                    .SelectMany(HierarchyLoader.downloadAndStoreDependencies);
            }
            else
            {
                throw new ArgumentException("unexpected type, cannot download");
            }

            return resultObs;
        }

        private IObservable<Event> IfNotDownloadedYet(Event ev)
        {
            if (ev == null)
                return Observable.Empty<Event>();

            return
            Observable.Return(ev)
                .Where(e =>
                {
                    if (!Mappings.ResolveToLocalKey(DBObjectType.Event, e.CollectionEventID.Value).HasValue)
                    {
                        return true;
                    }
                    else
                    {
                        Notifications.showNotification(DiversityResources.Download_EventAlreadyDownloaded);
                        return false;
                    }
                });
        }
    }
}