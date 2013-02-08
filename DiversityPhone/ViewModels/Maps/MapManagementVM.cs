using System;
using System.Net;
using System.Windows;

using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using System.Reactive.Linq;
using System.Linq;
using DiversityPhone.Services;
using System.Collections.Generic;
using Client = DiversityPhone.Model;
using DiversityPhone.DiversityService;
using ReactiveUI.Xaml;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using Funq;
using System.Reactive;

namespace DiversityPhone.ViewModels
{
    public class MapManagementVM : PageVMBase
    {
        public enum Pivot
        {
            Local,
            Online
        }

        private IConnectivityService Network;
        private IMapTransferService MapService;
        private IMapStorageService MapStorage;
        private INotificationService Notifications;

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

        public MapManagementVM(Container ioc)
        {
            Network = ioc.Resolve<IConnectivityService>();
            MapService = ioc.Resolve<IMapTransferService>();
            MapStorage = ioc.Resolve<IMapStorageService>();
            Notifications = ioc.Resolve<INotificationService>();



            this.FirstActivation()
                .Subscribe(_ => getMaps.Execute(null));

            MapList = getMaps.RegisterAsyncFunction(_ => MapStorage.getAllMaps().Select(m => new MapVM(m)))
                      .SelectMany(vms => vms)
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
                , x => x.IsOnlineAvailable, false);

            SearchMaps = new ReactiveAsyncCommand(_IsOnlineAvailable);

            _SearchResults = this.ObservableToProperty(
                SearchMaps.RegisterAsyncFunction(s => searchMapsImpl(s as string)),
                x => x.SearchResults);

            DownloadMap = new ReactiveCommand<MapVM>(vm => canBeDownloaded(vm as MapVM));
            DownloadMap
                .CheckConnectivity(Network, Notifications)
                .Do(vm => vm.IsDownloading = true)
                .Do(_ => CurrentPivot = Pivot.Local)                   
                .Do(vm => MapList.Add(vm))
                .Subscribe(downloadMap.Execute);

            downloadMap.RegisterAsyncObservable(vm => MapService.downloadMap((vm as MapVM).ServerKey))                
                .Select(map =>
                    {
                        var vm = (from v in MapList
                                  where v.ServerKey == map.ServerKey
                                  select v).SingleOrDefault();
                        if (vm != null)
                        {
                            vm.SetModel(map);
                        }
                        return vm;
                    })                
                .Subscribe(vm => SelectMap.CanExecute(vm));
        }

        private bool canBeDownloaded(MapVM vm)
        {
            return vm.Model == null && !vm.IsDownloading;
        }

        private IReactiveCollection<MapVM> searchMapsImpl(string p)
        {
            try
            {
                return new ReactiveCollection<MapVM>(MapService.GetAvailableMaps(p).First()
                    .Where(key => !_local_map_register.ContainsKey(key))
                    .Take(10)
                    .Select(mapname => new MapVM(null) { ServerKey = mapname }));
            }
            catch (Exception)
            {
                return new ReactiveCollection<MapVM>();
            }
        }
    }
}
