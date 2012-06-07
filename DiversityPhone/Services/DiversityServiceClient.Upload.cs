
using System.Collections.Generic;
using Client = DiversityPhone.Model;
using System;
using System.Reactive.Linq;
using DiversityPhone.DiversityService;
using System.Collections.ObjectModel;
using System.Linq;
using GlobalUtility;

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



        public IObservable<int> InsertEventSeries(Client.EventSeries series)
        {
            var res = Observable.FromEvent<EventHandler<InsertEventSeriesCompletedEventArgs>, InsertEventSeriesCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.InsertEventSeriesCompleted += d, d => _svc.InsertEventSeriesCompleted -= d)
                .Select(args => args.Result)
                .Take(1);
            var repoSeries = new ObservableCollection<EventSeries>();
            repoSeries.Add(Client.EventSeries.ToServiceObject(series));
            _svc.InsertEventSeriesAsync(repoSeries, this.GetCreds());
            return res.Select(dict => dict[series.SeriesID]);
        }
    }
}
