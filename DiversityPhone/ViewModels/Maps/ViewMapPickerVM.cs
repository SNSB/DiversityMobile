using System;
using System.Collections.Generic;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Services;
using DiversityPhone.Messages;
using DiversityPhone.Model;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DiversityPhone.ViewModels
{
    public class ViewMapPickerVM : PageViewModel
    {
        private IList<IDisposable> _subscriptions;
        private IMapStorageService _maps;

        #region Properties 

        private ObservableAsPropertyHelper<IList<MapVM>> _SavedMaps;
        public IList<MapVM> SavedMaps
        {
            get { return _SavedMaps.Value; }
        }

        private ISubject<ElementVMBase<Map>> MapSelected { get; set; }


        #endregion

        #region Commands
        public ReactiveCommand AddMaps { get; private set; }

        #endregion

        public ViewMapPickerVM(IMapStorageService maps)  
        {
            MapSelected = new Subject<ElementVMBase<Map>>();

            Messenger.RegisterMessageSource(
            MapSelected
                .Select(m => m.Model.ServerKey)
                .Select(uri =>
                    {
                        return new NavigationMessage(destinationFromState(CurrentState), uri, CurrentState.ReferrerType, CurrentState.Referrer);
                    })
                    );
                    
            _maps = maps;

            _SavedMaps = this.ObservableToProperty(
                StateObservable.Select(_ =>
                {
                    var res = _maps.getAllMaps().Select(m => new MapVM(m)).ToList();
                    res.ForEach(vm => vm.SelectObservable.Subscribe(MapSelected.OnNext));
                    return res as IList<MapVM>;
                })
                ,                
                x => x.SavedMaps);

            _subscriptions = new List<IDisposable>()
            {
                (AddMaps = new ReactiveCommand())
                    .Subscribe(_ => addMaps()),                      
            };

        }

        private void addMaps()       
        {
            Messenger.SendMessage<NavigationMessage>(new NavigationMessage(Page.DownLoadMaps, null));
        }

        private Page destinationFromState(PageState s)
        {
            Page destination;
            switch (s.ReferrerType)
            {
                case ReferrerType.EventSeries:
                    destination = Page.ViewMapES;
                    break;
                case ReferrerType.Event:
                    destination = Page.ViewMapEV;
                    break;
                case ReferrerType.IdentificationUnit:
                    destination = Page.ViewMapIU;
                    break;
                default:
                    destination = Page.ViewMap;
                    break;
            }

            return destination;
        }

        //private bool saveReferrer(PageState s)
        //{

        //    return true;
        //}
    }
}
