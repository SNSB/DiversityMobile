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
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{
    public class EventSeriesVM : ReactiveObject
    {
        public EventSeries Model { get; private set; }
        public string Description { get { return Model.Description; } }
        public ReactiveCommand SelectSeries { get; private set; }

        private IMessageBus _messenger;

        public EventSeriesVM(EventSeries model, IMessageBus messenger)
        {
            Model = model;
            _messenger = messenger;

            (SelectSeries = new ReactiveCommand())
                .Subscribe(_ => _messenger.SendMessage<EventSeries>(Model,MessageContracts.SELECT));
        }
    }
}
