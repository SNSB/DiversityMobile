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

        string _QueryString = "";
        public string QueryString
        {
            get { return _QueryString; }
            set { this.RaiseAndSetIfChanged(x => x.QueryString, ref _QueryString, value); }
        }

        private ObservableAsPropertyHelper<IEnumerable<MapVM>> _SearchResults;
        public IEnumerable<MapVM> SearchResults
        {
            get { return _SearchResults.Value; }
        }

        #endregion

        private ReactiveAsyncCommand getMaps = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand searchMaps;

        private ReactiveAsyncCommand downloadMap = new ReactiveAsyncCommand();

        public MapManagementVM(Container ioc)
        {            
            Network = ioc.Resolve<IConnectivityService>();
            MapService = ioc.Resolve<IMapTransferService>();
            MapStorage = ioc.Resolve<IMapStorageService>();

            

            this.FirstActivation()
                .Subscribe(_ => getMaps.Execute(null));

            MapList = getMaps.RegisterAsyncFunction(_ => MapStorage.getAllMaps().Select(m => new MapVM(m)))
                      .SelectMany(vms => vms)
                      .CreateCollection();
            MapList.ObserveCollectionChanged()
                .Subscribe(change =>
                    {
                        if(change.OldItems != null)
                            foreach (var item in change.OldItems)
                            {
                                var typed = item as MapVM;
                                if (typed != null)
                                    _local_map_register.Remove(typed.ServerKey);
                            }
                        if(change.NewItems != null)
                            foreach (var item in change.NewItems)
                            {
                                var typed = item as MapVM;
                                if (typed != null)
                                    _local_map_register.Add(typed.ServerKey, Unit.Default);
                            }                       
                    });

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

            _IsOnlineAvailable = this.ObservableToProperty(Network.WifiAvailable(), x => x.IsOnlineAvailable, false);

            searchMaps = new ReactiveAsyncCommand(_IsOnlineAvailable);

            this.ObservableForProperty(x => x.QueryString).ValueIfNotDefault()
                .Throttle(TimeSpan.FromSeconds(.5))
                .Where(s => s.Length > 2)
                .Where(s => searchMaps.CanExecute(s))
                .Subscribe(searchMaps.Execute);

            _SearchResults = this.ObservableToProperty(
                searchMaps.RegisterAsyncFunction(s => searchMapsImpl(s as string)),
                x => x.SearchResults);

            DownloadMap = new ReactiveCommand<MapVM>(vm => vm.Model == null && IsOnlineAvailable && downloadMap.CanExecute(vm));
            DownloadMap
                .Do(_ => CurrentPivot = Pivot.Local)                   
                .Do(vm => MapList.Add(vm))
                .Subscribe(downloadMap.Execute);

            downloadMap.RegisterAsyncFunction(vm =>
                {
                    var typed = vm as MapVM;

                    return new { Map = MapService.downloadMap(typed.ServerKey).First(), VM = typed };
                })
                .Do(tuple => tuple.VM.SetModel(tuple.Map))
                .Subscribe(tuple => SelectMap.CanExecute(tuple.VM));
        }

        private IEnumerable<MapVM> searchMapsImpl(string p)
        {
            try
            {                
                return MapService.GetAvailableMaps(p).First()
                    .Where(key => !_local_map_register.ContainsKey(key))
                    .Take(10)
                    .Select(mapname => new MapVM(null) { ServerKey = mapname });
            }
            catch(Exception)
            {
                return Enumerable.Empty<MapVM>();
            }
        }
    }
}
