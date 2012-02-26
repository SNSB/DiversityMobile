
using System.Collections.Generic;
using Client = DiversityPhone.Model;
using System;
using System.Reactive.Linq;
using DiversityPhone.DiversityService;
using System.Linq;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceObservableClient : IDiversityServiceClient
    {
        public IObservable<KeyProjection> InsertHierarchy(HierarchySection section)
        {
            var res = Observable.FromEvent<EventHandler<InsertHierarchyCompletedEventArgs>, InsertHierarchyCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.InsertHierarchyCompleted += d, d => _svc.InsertHierarchyCompleted -= d)
                .Select(args => args.Result)
                .Take(1);
            _svc.InsertHierarchyAsync(section, this.GetCreds());
            return res;
        }



        public IObservable<Dictionary<int, int>> InsertEventSeries(IEnumerable<Client.EventSeries> seriesList)
        {
            throw new NotImplementedException();
        }
    }
}
