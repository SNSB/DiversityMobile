namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using System;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public class DownloadVM : PageVMBase
    {
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

        public ReactiveCollection<Event> QueryResult { get; private set; }

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

            QueryResult = new ReactiveCollection<Event>();

            _IsOnlineAvailable = Connectivity.Status().Select(s => s == ConnectionStatus.Wifi).Do(_ => { this.GetType(); }, ex => { }, () => { })
                .ToProperty(this, x => x.IsOnlineAvailable);

            _HaveMaxResults = QueryResult.CollectionCountChanged
                .Select(c => c >= MAX_RESULTS)
                .ToProperty(this, x => x.HaveMaxResults, false);

            SearchEvents = new ReactiveAsyncCommand(this.WhenAny(x => x.IsOnlineAvailable, x => x.Value));
            SearchEvents.ShowInFlightNotification(Notifications, DiversityResources.Download_SearchingEvents);
            SearchEvents.ThrownExceptions
                    .ShowServiceErrorNotifications(Notifications)
                    .ShowErrorNotifications(Notifications)
                    .Subscribe();
            SearchEvents
                .RegisterAsyncObservable(query =>
                    Service.GetEventsByLocality(query as string ?? string.Empty)
                    .TakeUntil(this.OnDeactivation())
                )
                .Do(_ => QueryResult.Clear())
                .Subscribe(QueryResult.AddRange);

            CancelDownload = new ReactiveCommand();

            DownloadElement = new ReactiveAsyncCommand(this.WhenAny(x => x.IsOnlineAvailable, x => x.Value));
            DownloadElement.ThrownExceptions
                .ShowServiceErrorNotifications(Notifications)
                .ShowErrorNotifications(Notifications)
                .Subscribe();
            DownloadElement
                .RegisterAsyncObservable(ev => IfNotDownloadedYet(ev as Event)
                    .Select(HierarchyLoader.downloadAndStoreDependencies)
                    .SelectMany(dl => dl.TakeUntil(CancelDownload))
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