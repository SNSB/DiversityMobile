using System;
using DiversityPhone.Model;
using ReactiveUI;
using System.Reactive.Linq;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    

    public class EventVM : ElementVMBase<Event>
    {
        public override string Description { get { return Model.LocalityDescription; } }
        public override Icon Icon
        {
            get
            {
                return ViewModels.Icon.Event;
            }
        }
        protected override NavigationMessage NavigationMessage
        {
            get { return new NavigationMessage(TargetPage, Model.EventID.ToString()); }
        }

        public EventVM(IMessageBus _messenger, Event model, Page targetPage)
            : base(_messenger, model, targetPage)
        {

        }
    }
}
