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
using DiversityService.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{
    public class EventVM : ReactiveObject
    {
        private DiversityService.Model.Event model;
        private ReactiveUI.IMessageBus _messenger;

        public ReactiveCommand Select { get; private set; }

        public EventVM(Event model, IMessageBus _messenger)
        {
            this.model = model;
            this._messenger = _messenger;

            (Select = new ReactiveCommand())
                .Subscribe(_ =>
                    {
                        _messenger.SendMessage<Event>(model, MessageContracts.SELECT);
                    });
                        
        }

    }
}
