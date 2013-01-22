using DiversityPhone.Model;
using DiversityPhone.Services;
using Funq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels.Utility
{
    public class DownloadVM : ReactiveObject
    {
        private readonly IDiversityServiceClient Service;
        private readonly INotificationService Notifications;

        public DownloadVM(Container ioc)
        {
            Service = ioc.Resolve<IDiversityServiceClient>();
            Notifications = ioc.Resolve<INotificationService>();

            QueryResult = new ReactiveCollection<IElementVM<Event>>();

            var queryStrings = this.ObservableForProperty(x => x.EventQuery).Value();
            queryStrings
                .Throttle(TimeSpan.FromMilliseconds(500));
                
                
        }


        private string _EventQuery;

        public string EventQuery
        {
            get
            {
                return _EventQuery;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.EventQuery, ref _EventQuery, value);
            }
        }

        public ReactiveCollection<IElementVM<Event>> QueryResult { get; private set; }
        

    }
}
