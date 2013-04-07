using DiversityPhone.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using ReactiveUI.Xaml;
using System.Reactive;
using System.Threading.Tasks;
using System.Reactive.Joins;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using Ninject;
using DiversityPhone.Interface;
using System.Reactive.Concurrency;


namespace DiversityPhone.ViewModels
{
    [Flags]
    enum ProgressType
    {
        Total = 0x01, Completed = 0x02,
        TotalAndCompleted = Total | Completed
    }
    static class ProgressExtensions
    {

        public static IObservable<T> CountProgress<T, U>(this IObservable<T> This, ProgressReporter p, ProgressType t) where T : IEnumerable<U>
        {
            return This.Do(l =>
            {
                var count = l.Count();
                if ((t & ProgressType.Total) == ProgressType.Total) p.Total += count;
                if ((t & ProgressType.Completed) == ProgressType.Completed) p.Completed += count;
            });
        }

        public static IObservable<T> CountProgress<T>(this IObservable<T> This, ProgressReporter p, ProgressType t)
        {
            return This.Do(l =>
            {
                if ((t & ProgressType.Total) == ProgressType.Total) p.Total++;
                if ((t & ProgressType.Completed) == ProgressType.Completed) p.Completed++;
            });
        }
    }

    internal class ProgressReporter
    {
        private int _Total;

        public int Total
        {
            get { return _Total; }
            set
            {
                _Total = value;
                pushUpdate();
            }
        }

        private int _Completed;

        public int Completed
        {
            get { return _Completed; }
            set { _Completed = value; pushUpdate(); }
        }

        private void pushUpdate()
        {
            _inner.OnNext(string.Format("{0} {1}/{2}", DiversityResources.Download_DownloadingItems, Completed, Total));
        }

        public bool Cancelled { get; private set; }

        private ISubject<string> _inner = new Subject<string>();

        public ProgressReporter()
        {

        }
    }

    public class DownloadVM : PageVMBase
    {
        private readonly IDiversityServiceClient Service;
        private readonly INotificationService Notifications;
        private readonly IConnectivityService Connectivity;
        private readonly IFieldDataService Storage;
        private readonly IKeyMappingService Mappings;
        private readonly EventHierarchyLoader HierarchyLoader;


        public bool IsDownloading { get { return _IsDownloading.Value; } }
        private ObservableAsPropertyHelper<bool> _IsDownloading;


        public bool IsOnlineAvailable { get { return _IsOnlineAvailable.Value; } }
        private ObservableAsPropertyHelper<bool> _IsOnlineAvailable;

        public ReactiveAsyncCommand SearchEvents { get; private set; }

        public ReactiveCollection<Event> QueryResult { get; private set; }

        public ReactiveAsyncCommand DownloadElement { get; set; }

       
        public DownloadVM(
            IDiversityServiceClient Service,
            INotificationService Notifications,
            IConnectivityService Connectivity,
            IFieldDataService Storage,
            IKeyMappingService Mappings,
            EventHierarchyLoader HierarchyLoader
            )
        {
            this.Service = Service;
            this.Notifications = Notifications;
            this.Connectivity = Connectivity;
            this.Storage = Storage;
            this.Mappings = Mappings;
            this.HierarchyLoader = HierarchyLoader;

            QueryResult = new ReactiveCollection<Event>();

            _IsOnlineAvailable = Connectivity.Status().Select(s => s == ConnectionStatus.Wifi).Do(_ => { this.GetType(); }, ex => { }, () => { })
                .ToProperty(this, x => x.IsOnlineAvailable);

            SearchEvents = new ReactiveAsyncCommand(this.WhenAny(x => x.IsOnlineAvailable, x => x.Value));
            SearchEvents
                .RegisterAsyncObservable(query =>
                    Service.GetEventsByLocality(query as string ?? string.Empty)
                    .HandleServiceErrors(Notifications, Messenger, Observable.Empty<IEnumerable<Event>>())
                    .DisplayProgress(Notifications, DiversityResources.Download_SearchingEvents)
                    .TakeUntil(this.OnDeactivation())
                )
                .Do(_ => QueryResult.Clear())
                .Subscribe(QueryResult.AddRange);

            _IsDownloading = SearchEvents.ItemsInflight
                .Select(x => x > 0)
                .ToProperty(this, x => x.IsDownloading);

            DownloadElement = new ReactiveAsyncCommand(this.WhenAny(x => x.IsOnlineAvailable, x => x.Value));
            DownloadElement
                .RegisterAsyncObservable(ev => IfNotDownloadedYet(ev as Event)
                    .SelectMany(HierarchyLoader.downloadAndStoreDependencies));

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
