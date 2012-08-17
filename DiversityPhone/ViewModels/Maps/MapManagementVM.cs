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

namespace DiversityPhone.ViewModels
{
    public class MapManagementVM : PageVMBase
    {
        private IMessageBus Messenger;
        private IMapTransferService MapService;
        private IMapStorageService MapStorage;
       
        public ReactiveCommand<MapVM> SelectMap { get; private set; }
        public ReactiveCommand<MapVM> DeleteMap { get; private set; }
        public ReactiveCommand SearchOnline { get; set; }
        public ReactiveCommand<MapVM> DownloadMap { get; private set; }

        #region Properties

        public IReactiveCollection<MapVM> MapList { get; private set; }


        #endregion

        private ReactiveAsyncCommand getMaps = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand downloadMap;

        public MapManagementVM(Container ioc)
        {
            Messenger = ioc.Resolve<IMessageBus>();
            MapService = ioc.Resolve<IMapTransferService>();
            MapStorage = ioc.Resolve<IMapStorageService>();

            downloadMap = new ReactiveAsyncCommand();

            this.OnFirstActivation(() => getMaps.Execute(null));

            MapList = getMaps.RegisterAsyncFunction(_ => MapStorage.getAllMaps().Select(m => new MapVM(m)))
                      .SelectMany(vms => vms)
                      .CreateCollection();

            SelectMap = new ReactiveCommand<MapVM>(vm => !vm.IsDownloading);
            SelectMap.Select(vm => vm as IElementVM<Map>)
                .ToMessage(MessageContracts.VIEW);


        }
    }
}
