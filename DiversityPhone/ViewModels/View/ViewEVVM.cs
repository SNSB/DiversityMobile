namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using DiversityPhone.Messages;
    using System.Reactive.Linq;
    using DiversityPhone.Services;
    using ReactiveUI.Xaml;

    public class ViewEVVM : PageViewModel
    {
        IList<IDisposable> _subscriptions;

        #region Services
        IMessageBus _messenger;
        IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand Add { get; private set; }
        #endregion

        #region Properties
        public EventVM Current { get { return _Current.Value; } }
        private ObservableAsPropertyHelper<EventVM> _Current;        

        public IList<SpecimenVM> SpecList { get { return _SpecList.Value; } }
        private ObservableAsPropertyHelper<IList<SpecimenVM>> _SpecList;
        #endregion




        public ViewEVVM(IMessageBus messenger, IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;



            var rawModel = StateObservable
                .Select(s => EventFromContext(s.Context));
            var modelDeleted = rawModel.Select(ev => ev == null);
            var validModel = rawModel.Where(ev => ev != null);

            _messenger.RegisterMessageSource(modelDeleted.Select(_ => Message.NavigateBack));

            _Current = validModel
                .Select(ev => new EventVM(_messenger, ev))
                .ToProperty(this, x => x.Current);

            _SpecList = validModel               
                .Select(ev => getSpecimenList(ev))
                .ToProperty(this, x => x.SpecList);



            _subscriptions = new List<IDisposable>()
            {
                (Add = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<NavigationMessage>(
                        new NavigationMessage(Page.EditIU, null)
                        /*new IdentificationUnit()
                        {
                            SpecimenID = Current.Model.EventID
                        }*/
                        )),                        
                
            };
        }

        private IList<SpecimenVM> getSpecimenList(Event ev)
        {
            return new VirtualizingReadonlyViewModelList<Specimen, SpecimenVM>(
                _storage.getSpecimenForEvent(ev),
                (model) => new SpecimenVM(_messenger, model)
                );
        }

        private Event EventFromContext(string ctx)
        {
            if (ctx != null)
            {
                int id;
                if (int.TryParse(ctx, out id))
                {
                    return _storage.getEventByID(id);
                }
            }
            return null;
        }
    }
}
