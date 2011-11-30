namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using System.Reactive.Linq;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using DiversityPhone.Messages;
    using DiversityPhone.Services;
    using ReactiveUI.Xaml;
using System.Reactive.Subjects;

    public class ViewIUVM : PageViewModel
    {
        IList<IDisposable> _subscriptions;

        #region Services
        IMessageBus _messenger;
        IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand AddSubunit { get; private set; }
        #endregion

        #region Properties
        public IdentificationUnitVM Current { get { return _Current.Value; } }
        private ObservableAsPropertyHelper<IdentificationUnitVM> _Current;

        //Liste Subunits
        
        #endregion

        

        public ViewIUVM(IMessageBus messenger, IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;            

            _Current = StateObservable
                .Select(s=>getUnitFromContext(s.Context))
                .Where(iu => iu != null)
                .Select(iu => getSubUnits(iu))
                .ToProperty(this, x => x.Current);

            var newSubUnits = (AddSubunit = new ReactiveCommand())
                                .Select(_ => new NavigationMessage(Page.EditIU,null));
            _messenger.RegisterMessageSource(newSubUnits);



            _subscriptions = new List<IDisposable>()
            {

            };
        }

        private IdentificationUnit getUnitFromContext(string ctx)
        {
            if (ctx != null)
            {
                int id;
                if (int.TryParse(ctx, out id))
                {
                    return _storage.getIdentificationUnitByID(id);
                }               
            }
            return null;
        } 
      
        private IdentificationUnitVM getSubUnits(IdentificationUnit iu)
        {
            return new IdentificationUnitVM(
                _messenger,
                iu,
                IdentificationUnitVM.getTwoLevelVMFromModelList(
                    _storage.getSubUnits(iu),
                    unit => _storage.getSubUnits(unit),
                    _messenger));
                    
        }
    }
}
