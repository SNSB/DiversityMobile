using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using System.Collections.Generic;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{

    public class IdentificationUnitVM : ReactiveObject
    {
        IMessageBus _messenger;
        IList<IdentificationUnitVM> _ubunits;
        IList<IDisposable> _subscriptions;

        public IdentificationUnit Model { get; private set; }
        public string Description { get { return Model.UnitID.ToString(); } }

        public IList<IdentificationUnitVM> SubUnits { get; private set; }
        public bool HasSubUnits { get { return (SubUnits != null)?SubUnits.Count > 0:false; } }


        public ReactiveCommand Select { get; private set; }

        public IdentificationUnitVM(IMessageBus messenger, IdentificationUnit model, IList<IdentificationUnitVM> subunits)
        { 
            _messenger = messenger;
            SubUnits = subunits;
            Model = model;

            _subscriptions = new List<IDisposable>()
            {
                (Select = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<IdentificationUnit>(Model, MessageContracts.SELECT))
            };
        }

        public static IList<IdentificationUnitVM> getTwoLevelVMFromModelList(IList<IdentificationUnit> source, Func<IdentificationUnit,IList<IdentificationUnit>> getSubUnits, IMessageBus messenger)
        {
            return getVMListFromModelAndFactory(source,
                iu => new IdentificationUnitVM(
                    messenger, 
                    iu, 
                    getSingleLevelVMFromModelList(getSubUnits(iu),messenger)
                    ));
        }
        public static IList<IdentificationUnitVM> getSingleLevelVMFromModelList(IList<IdentificationUnit> source, IMessageBus messenger)
        {
            return getVMListFromModelAndFactory(source,
                iu => new IdentificationUnitVM(messenger, iu, null));
        }

        private static IList<IdentificationUnitVM> getVMListFromModelAndFactory(IList<IdentificationUnit> source, Func<IdentificationUnit,IdentificationUnitVM> vmFactory)
        {
            return new VirtualizingReadonlyViewModelList<IdentificationUnit, IdentificationUnitVM>(
                        source,
                        vmFactory
                        );
        }
    }
        
}
