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
using System.Collections.Generic;

namespace DiversityPhone.ViewModels
{
    public class AnalysisVM:ReactiveObject
    {
        IList<IDisposable> _subscriptions;

        IMessageBus _messenger;

        public ReactiveCommand Select { get; private set; }
        public ReactiveCommand Edit { get; private set; }

        public IdentificationUnitAnalysis Model { get; private set; }
        public string Description { get { return Model.ToString(); } }
        public Icon Icon { get; private set; }

        public AnalysisVM(IdentificationUnitAnalysis model, IMessageBus messenger)
        {
            _messenger = messenger;
            Model = model;
            Icon=ViewModels.Icon.Analysis;
            _subscriptions = new List<IDisposable>()
            {
                (Select = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<IdentificationUnitAnalysis>(Model,MessageContracts.SELECT)),
                (Edit = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<IdentificationUnitAnalysis>(Model,MessageContracts.EDIT)),
            };
        }

    }
}
