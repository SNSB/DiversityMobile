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
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;

namespace DiversityPhone.ViewModels
{
    public class EventVM : ReactiveObject
    {
        public Event Model {get; private set;}
        private IMessageBus _messenger;
        private IList<IDisposable> _subscriptions;

        public string Description { get { return Model.LocalityDescription; } }

        public ReactiveCommand Select { get; private set; }

        public EventVM(Event model, IMessageBus _messenger)
        {
            Model = model;
            this._messenger = _messenger;

            _subscriptions = new List<IDisposable>()
            {
                (Select = new ReactiveCommand())
                    .Subscribe(_ =>
                        {
                            _messenger.SendMessage<Event>(model, MessageContracts.SELECT);
                        })
            };
                        
        }

    }
}
