using DiversityPhone.Interface;
using DiversityPhone.Model;
using DiversityPhone.Services;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public class MapManagementVM : PageVMBase
    {
        public enum Pivot
        {
            Local,
            Online
        }

        readonly IConnectivityService Network;
        readonly IMapTransferService MapService;
        readonly IMapStorageService MapStorage;
        readonly INotificationService Notifications;

        public ReactiveAsyncCommand SearchMaps { get; private set; }
        public ReactiveCommand<MapVM> SelectMap { get; private set; }
        public ReactiveCommand<MapVM> DeleteMap { get; private set; }
        public ReactiveCommand<MapVM> DownloadMap { get; private set; }


        private IDictionary<string, Unit> _local_map_register = new Dictionary<string, Unit>();
        #region Properties


        private Pivot _CurrentPivot;
        public Pivot CurrentPivot
        {
            get
            {
                return _CurrentPivot;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.CurrentPivot, ref _CurrentPivot, value);
            }
        }


        public ReactiveCollection<MapVM> MapList { get; private set; }


        private ObservableAsPropertyHelper<bool> _IsOnlineAvailable;
        public bool IsOnlineAvailable
        {
            get { return _IsOnlineAvailable.Value; }
        }

        private ObservableAsPropertyHelper<IReactiveCollection<MapVM>> _SearchResults;
        public IReactiveCollection<MapVM> SearchResults
        {
            get { return _SearchResults.Value; }
        }

        #endregion

        private ReactiveAsyncCommand getMaps = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand downloadMap = new ReactiveAsyncCommand();

        public MapManagementVM(
            IConnectivityService Network,
            IMapTransferService MapService,
            IMapStorageService MapStorage,
            INotificationService Notifications,
            [Dispatcher] IScheduler Dispatcher
            )
        {
            Contract.Requires(Network != null);
            Contract.Requires(MapService != null);
            Contract.Requires(MapStorage != null);
            Contract.Requires(Notifications != null);
            this.Network = Network;
            this.MapService = MapService;
            this.MapStorage = MapStorage;
            this.Notifications = Notifications;



            this.FirstActivation()
                .Subscribe(_ => getMaps.Execute(null));

            MapList = getMaps.RegisterAsyncFunction(_ => MapStorage.getAllMaps().Select(m => new MapVM(m)))
                      .SelectMany(vms => vms.ToList())
                      .ObserveOn(Dispatcher)
                      .CreateCollection();
            MapList.ItemsAdded
                .Subscribe(item => _local_map_register.Add(item.ServerKey, Unit.Default));

            MapList.ItemsRemoved
                .Subscribe(item => _local_map_register.Remove(item.ServerKey));

            SelectMap = new ReactiveCommand<MapVM>(vm => !vm.IsDownloading);
            SelectMap
                .Select(vm => vm as IElementVM<Map>)
                .ToMessage(MessageContracts.VIEW);

            SelectMap
                .Select(_ => Page.Previous)
                .ToMessage();

            DeleteMap = new ReactiveCommand<MapVM>(vm => !vm.IsDownloading);
            DeleteMap
                .Do(vm => MapList.Remove(vm))
                .Select(vm => vm.Model)
                .Where(map => map != null)
                .Subscribe(map => MapStorage.deleteMap(map));

            _IsOnlineAvailable = this.ObservableToProperty(
                this.OnActivation()
                .SelectMany(Network.WifiAvailable().TakeUntil(this.OnDeactivation()))
                .Do(x => {})
                , x => x.IsOnlineAvailable, false);

            SearchMaps = new ReactiveAsyncCommand(_IsOnlineAvailable);

            _SearchResults = this.ObservableToProperty<MapManagementVM, IReactiveCollection<MapVM>>(
                SearchMaps.RegisterAsyncFunction(s => searchMapsImpl(s as string))
                .ObserveOn(Dispatcher)
                .Select(result =>
                    {
                        try
                        {
                            return new ReactiveCollection<MapVM>(result.Select(x => new MapVM(null) { ServerKey = x })) as IReactiveCollection<MapVM>;
                        }
                        catch (Exception ex)
                        {
                            return null;
                        }
                    }),
                x => x.SearchResults);

            DownloadMap = new ReactiveCommand<MapVM>(vm => canBeDownloaded(vm as MapVM), Observable.Empty<Unit>());
            DownloadMap
                .CheckConnectivity(Network, Notifications)
                .Do(vm => vm.IsDownloading = true)
                .Do(_ => CurrentPivot = Pivot.Local)
                .Do(vm => MapList.Add(vm))
                .Subscribe(downloadMap.Execute);

            downloadMap.RegisterAsyncObservable(vm =>
            {
                var vm_t = vm as MapVM;
                if (vm_t == null)
                    return Observable.Empty<System.Tuple<MapVM, Map>>();
                else
                    return MapService.downloadMap(vm_t.ServerKey)
                        .HandleServiceErrors(Notifications, Messenger, Observable.Return<Map>(null))
                        .Catch((WebException ex) =>
                        {
                            Messenger.SendMessage(new DialogMessage(DialogType.OK, DiversityResources.Message_SorryHeader, DiversityResources.MapManagement_Message_NoPermissions));

                            return Observable.Return<Map>(null);
                        })
                        .Select(map => System.Tuple.Create(vm_t, map));
            })
            .ObserveOn(Dispatcher)
            .Select(t =>
                {
                    if (t.Item1 != null) // VM
                    {
                        if (t.Item2 != null) // Map
                        {
                            t.Item1.SetModel(t.Item2);
                        }
                        else
                        {
                            MapList.Remove(t.Item1);
                        }
                    }
                    return t.Item1;
                }).Subscribe(_ => SelectMap.RaiseCanExecuteChanged());
        }

        private bool canBeDownloaded(MapVM vm)
        {
            return vm.Model == null && !vm.IsDownloading;
        }

        private IEnumerable<string> searchMapsImpl(string p)
        {
            try
            {
                return
                MapService.GetAvailableMaps(p).First()
                    .Where(key => !_local_map_register.ContainsKey(key))
                    .Take(10)
                    .ToList();
            }
            catch (Exception)
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}
