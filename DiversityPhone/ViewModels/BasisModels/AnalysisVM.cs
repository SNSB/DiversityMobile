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
        protected override NavigationMessage NavigationMessage
        {
            get { return new NavigationMessage(TargetPage, Model.IdentificationUnitAnalysisID.ToString()); }
        }

        public IUAnalysisVM(IMessageBus _messenger, IdentificationUnitAnalysis model, Page targetPage)
            : base(_messenger, model, targetPage)
        {

        }
    }
}
