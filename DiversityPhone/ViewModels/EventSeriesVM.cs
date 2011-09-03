﻿using System;
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
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;

namespace DiversityPhone.ViewModels
{
    public class EventSeriesVM : ReactiveObject
    {
        IMessageBus _messenger;
        IList<IDisposable> _subscriptions;

        public EventSeries Model { get; private set; }
        public string Description { get { return Model.Description; } }

        public ReactiveCommand Select { get; private set; }
        public ReactiveCommand Edit { get; private set; }

        public EventSeriesVM(EventSeries model, IMessageBus messenger)
        {
            _messenger = messenger;
            Model = model;

            _subscriptions = new List<IDisposable>()
            {
                (Select = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<EventSeries>(Model,MessageContracts.SELECT)),
                (Edit = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<EventSeries>(Model,MessageContracts.EDIT)),
            };
        }
    }
}
        
        

   
