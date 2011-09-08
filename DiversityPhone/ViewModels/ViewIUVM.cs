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
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{

    public class ViewIUVM : ReactiveObject
    {
        IMessageBus _messenger;
        IList<IDisposable> _subscriptions;



        public ViewIUVM(IMessageBus messenger)
        {
            _messenger = messenger;

            _subscriptions = new List<IDisposable>()
            {
                _messenger.Listen<IdentificationUnit>(MessageContracts.SELECT)
                    .Subscribe(iu => selectIU(iu)),
            };
        }

        private void selectIU(IdentificationUnit iu)
        {

        }
    }
        
}
