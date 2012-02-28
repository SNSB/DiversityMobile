
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



        public IObservable<Dictionary<int, int>> InsertEventSeries(IEnumerable<EventSeries> seriesList)
        {
            var res = Observable.FromEvent<EventHandler<InsertEventSeriesCompletedEventArgs>, InsertEventSeriesCompletedEventArgs>((a) => (s, args) => a(args), d => _svc.InsertEventSeriesCompleted += d, d => _svc.InsertEventSeriesCompleted -= d)
                .Select(args => args.Result)
                .Take(1);
            ObservableCollection<EventSeries> seriesConv = ObservableConverter.ToObservableCollection<EventSeries>(seriesList);
            _svc.InsertEventSeriesAsync(seriesConv);
            return res;
        }
    }
}
