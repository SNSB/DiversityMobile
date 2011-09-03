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

namespace DiversityPhone.ViewModels
{

    public class IdentificationUnitVM : ReactiveObject
    {
        IMessageBus _messenger;
        IList<IDisposable> _subscriptions;

        public IdentificationUnit Model { get; private set; }
        public string Description { get { return Model.UnitID.ToString(); } }


        public ReactiveCommand Select { get; private set; }

        public IdentificationUnitVM(IMessageBus messenger, IdentificationUnit model)
        { 
            _messenger = messenger;
            Model = model;

            _subscriptions = new List<IDisposable>()
            {
                (Select = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<IdentificationUnit>(Model, MessageContracts.SELECT))
            };
        }
    }
        
}
