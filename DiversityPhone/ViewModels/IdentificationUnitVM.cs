namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using ReactiveUI.Xaml;
    using DiversityPhone.Messages;

    public class IdentificationUnitVM : ReactiveObject
    {
        IList<IDisposable> _subscriptions;

        IMessageBus _messenger;

        public ReactiveCommand Select { get; private set; }
        public ReactiveCommand Edit { get; private set; }


        public IdentificationUnit Model { get; private set; }
        public string Description { get { return string.Format("[{0}] {1}", Model.UnitID, Model.UnitDescription ?? ""); } }

        public IList<IdentificationUnitVM> SubUnits { get; private set; }
        public bool HasSubUnits { get { return (SubUnits != null) ? SubUnits.Count > 0 : false; } }


        public IdentificationUnitVM(IMessageBus messenger, IdentificationUnit model, IList<IdentificationUnitVM> subunits)
        {
            if (messenger == null) throw new ArgumentNullException("messenger");
            if (model == null) throw new ArgumentNullException("model");


            _messenger = messenger;
            SubUnits = subunits;
            Model = model;

            _subscriptions = new List<IDisposable>()
            {
                (Select = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<IdentificationUnit>(Model, MessageContracts.SELECT)),
                (Edit = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<IdentificationUnit>(Model, MessageContracts.EDIT)),

            };
        }

        public static IList<IdentificationUnitVM> getTwoLevelVMFromModelList(IList<IdentificationUnit> source, Func<IdentificationUnit, IList<IdentificationUnit>> getSubUnits, IMessageBus messenger)
        {
            return getVMListFromModelAndFactory(source,
                iu => new IdentificationUnitVM(
                    messenger,
                    iu,
                    getSingleLevelVMFromModelList(getSubUnits(iu), messenger)
                    ));
        }
        public static IList<IdentificationUnitVM> getSingleLevelVMFromModelList(IList<IdentificationUnit> source, IMessageBus messenger)
        {
            return getVMListFromModelAndFactory(source,
                iu => new IdentificationUnitVM(messenger, iu, null));
        }

        private static IList<IdentificationUnitVM> getVMListFromModelAndFactory(IList<IdentificationUnit> source, Func<IdentificationUnit, IdentificationUnitVM> vmFactory)
        {
            return new VirtualizingReadonlyViewModelList<IdentificationUnit, IdentificationUnitVM>(
                        source,
                        vmFactory
                        );
        }
    }

}
