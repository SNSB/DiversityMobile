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

        public EventVM(Event model)
            : base(model)
        {
            model.ObservableForProperty(x => x.LocalityDescription)
                .Subscribe(_ => this.RaisePropertyChanged(x => x.Description));
        }
    }
}
