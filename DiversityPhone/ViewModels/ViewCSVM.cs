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

    public class ViewCSVM : ReactiveObject
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
        public SpecimenVM Current { get { return _Current.Value; } }
        private ObservableAsPropertyHelper<SpecimenVM> _Current;

        //List<IdentificationUnitVM> Units;
        #endregion



        public ViewCSVM(IMessageBus messenger, IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;

            _Current = _messenger.Listen<Specimen>(MessageContracts.SELECT)
                .Select(spec => new SpecimenVM(_messenger,spec))
                .ToProperty(this, x => x.Current);


            _subscriptions = new List<IDisposable>()
            {

            };
        }

    }
}
