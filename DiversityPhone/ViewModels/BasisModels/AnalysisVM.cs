using ReactiveUI;
using System.Collections.Generic;
using ReactiveUI.Xaml;
using System;
using System.Reactive.Linq;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class IUAnalysisVM : ElementVMBase<IdentificationUnitAnalysis>
    {
        public override string Description { get { return Model.ToString(); } }
        public override Icon Icon
        {
            get
            {
                return Icon.Analysis;
            }
        }

        public IUAnalysisVM(IdentificationUnitAnalysis model, IMessageBus messenger)
            : base(messenger, model)
        {
            Edit = new ReactiveCommand();
            Messenger.RegisterMessageSource(
                Edit
                .Select(_ => new NavigationMessage(Page.EditIUAN, Model.AnalysisID.ToString()))
                );                
            
        }

    }
}
