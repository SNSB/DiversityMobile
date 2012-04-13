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

namespace DiversityPhone.ViewModels
{
    public class MapManagementVM :PageViewModel
    {
        public enum Pivot
        {
            Local,
            Repository
        }

        #region Services
        private IMapTransferService MapDownload { get; set; }
        #endregion

        #region Commands

        public ReactiveCommand Select { get; private set; }

        #endregion

        #region Properties

        private Pivot _CurrentPivot = Pivot.Local;
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

        public bool IsBusy { get { return _IsBusy.Value; } }
        private ObservableAsPropertyHelper<bool> _IsBusy;

        private String _SearchString;
        public String SearchString
        {
            get { return _SearchString; }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SearchString, ref _SearchString, value);
            }
        }

        public ObservableCollection<ManagedMapVM> LocalMaps { get; private set; }
        public ObservableCollection<ManagedMapVM> AvailableMaps { get; private set; } 
     
        private ReactiveAsyncCommand getAvailableMaps = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getLocalMaps = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand downloadMap = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand deleteMap = new ReactiveAsyncCommand();

        public ReactiveCommand Download { get; private set; }
        public ReactiveCommand Delete { get; private set; }

        

        #endregion

        public MapManagementVM(IMessageBus messenger, IMapTransferService maps)
            : base(messenger)
        {            
            MapDownload = maps;

            _IsBusy = Observable.Merge(
                deleteMap.ItemsInflight.Select(count => count > 0),
                downloadMap.ItemsInflight.Select(count => count > 0)
                ).ToProperty(this, x => x.IsBusy, false);

            Download = new ReactiveCommand(_IsBusy.Select(x => !x));
            Download
                .Where(arg => arg is ManagedMapVM)
                .Select(arg => arg as ManagedMapVM)
                .Subscribe(map =>
                        {
                                if (downloadMap.CanExecute(map))
                                {
                                    AvailableMaps.Remove(map);
                                    map.IsDownloading = true;
                                    downloadMap.Execute(map);
                                    LocalMaps.Add(map);
                                    CurrentPivot = Pivot.Local;
                                }
                        });

            Delete = new ReactiveCommand(_IsBusy.Select(x => !x));
            Delete
                .Where(argument => argument is ManagedMapVM)
                .Select(argument => argument as ManagedMapVM)
                .Subscribe(map =>
                    {
                        if (!map.IsDownloading && deleteMap.CanExecute(map))
                        {
                            deleteMap.Execute(map);
                            LocalMaps.Remove(map);
                        }
                    });

            LocalMaps =
                getLocalMaps
                .RegisterAsyncFunction(_=> getLocalMapsImpl())
                .SelectMany(selections => selections)
                .Select(selection =>
                    {
                        return new ManagedMapVM(selection);
                    }
                ).CreateCollection();
             

            AvailableMaps = 
                getAvailableMaps
                    .RegisterAsyncFunction(_ => getAvailableMapsImpl(SearchString))
                    .SelectMany(availableMaps => availableMaps.Select(map => new ManagedMapVM(map) { IsDownloading = false, Uri=null}))
                    .CreateCollection();

            downloadMap
                .RegisterAsyncFunction(arg => downloadMapImpl((arg as ManagedMapVM)))           
                .Subscribe(downloadedMap => 
                    {
                        downloadedMap.IsDownloading = false;
                    });

            deleteMap
                .RegisterAsyncFunction(arg => deleteMapImpl(arg as ManagedMapVM));               
                     
        }

        private ManagedMapVM downloadMapImpl(ManagedMapVM map)
        {
            throw new NotImplementedException();
            //Map loadedMap = MapDownload.downloadMap(map.ServerKey).Value;
            //map.Uri = loadedMap.Uri;
            //return map;
        }

        private IEnumerable<String> getAvailableMapsImpl(String searchString)
        {
            throw new NotImplementedException();
            //return MapDownload.getAvailableMaps(searchString).Value;
        }


        private IEnumerable<String> getLocalMapsImpl()
        {
            IList<Map> maps = MapDownload.getAllMaps();
            List<String> mapKeys=new List<string>();
            foreach (Map map in maps)
                mapKeys.Add(map.ServerKey);
            return mapKeys;
        }

        private bool deleteMapImpl(ManagedMapVM manMap)
        {
            MapDownload.deleteMap(MapDownload.getMapbyServerKey(manMap.ServerKey));
            return true;
        }

        private void selectImpl(ManagedMapVM manMap)
        {
            Map map = null;
            try
            {
                map = MapDownload.getMapbyServerKey(manMap.ServerKey);
            }
            catch (Exception)
            {
                map = null;
            }
            if (map != null)
            {
                NavigationMessage nav=new NavigationMessage(Page.ViewMap,map.ServerKey,ReferrerType.None,null);
                Messenger.SendMessage<NavigationMessage>(nav);
            }
        }

    }
}
