using ReactiveUI;
using System.Collections.Generic;
using ReactiveUI.Xaml;
using System;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class AnalysisVM : ReactiveObject
    {
        IList<IDisposable> _subscriptions;

        IMessageBus _messenger;
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
                (Edit = new ReactiveCommand())
                    .Subscribe(_ =>
                        {
                            _messenger.SendMessage<NavigationMessage>(new NavigationMessage(Page.EditIUAN, Model.AnalysisID.ToString()));
                        }),                
            };
        }

    }
}
