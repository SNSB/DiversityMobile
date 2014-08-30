using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient : IDiversityServiceClient
    {
        public IObservable<EventSeries> GetEventSeriesByID(int seriesID)
        {
            object request = new object();
            var res = from result in EventSeriesByIDCompleted.MakeObservableServiceResultSingle(request)
                      select result.Result.ToClientObject();
            _svc.EventSeriesByIDAsync(seriesID, GetCreds(), request);
            return res;
        }

        public IObservable<IEnumerable<Localization>> GetEventSeriesLocalizations(int seriesID)
        {
            object request = new object();
            var res = from result in LocalizationsForSeriesCompleted.MakeObservableServiceResultSingle(request)
                      select from loc in result.Result
                             select loc.ToClientObject();
            _svc.LocalizationsForSeriesAsync(seriesID, GetCreds(), request);
            return res;
        }

        public IObservable<IEnumerable<Event>> GetEventsByLocality(string localityQuery)
        {
            object request = new object();
            var res = from result in EventsByLocalityCompleted.MakeObservableServiceResultSingle(request)
                      select from ev in result.Result
                             select ev.ToClientObject(ev.CollectionSeriesID);
            _svc.EventsByLocalityAsync(localityQuery, GetCreds(), request);
            return res;
        }

        public IObservable<IEnumerable<EventProperty>> GetEventProperties(int eventID)
        {
            object request = new object();
            var res = from result in PropertiesForEventCompleted.MakeObservableServiceResultSingle(request)
                      select from p in result.Result
                             select p.ToClientObject();
            _svc.PropertiesForEventAsync(eventID, GetCreds(), request);
            return res;
        }

        public IObservable<IEnumerable<Specimen>> GetSpecimenForEvent(int eventID)
        {
            object request = new object();
            var res = from result in SpecimenForEventCompleted.MakeObservableServiceResultSingle(request)
                      select from spec in result.Result
                             select spec.ToClientObject();
            _svc.SpecimenForEventAsync(eventID, GetCreds(), request);
            return res;
        }

        public IObservable<IEnumerable<IdentificationUnit>> GetIdentificationUnitsForSpecimen(int specimenID)
        {
            object request = new object();
            var res = from result in UnitsForSpecimenCompleted.MakeObservableServiceResultSingle(request)
                      select from iu in result.Result
                             select iu.ToClientObject();
            _svc.UnitsForSpecimenAsync(specimenID, GetCreds(), request);
            return res;
        }

        public IObservable<IEnumerable<IdentificationUnit>> GetSubUnitsForIU(int unitID)
        {
            object request = new object();
            var res = from result in SubUnitsForIUCompleted.MakeObservableServiceResultSingle(request)
                      select from iu in result.Result
                             select iu.ToClientObject();
            _svc.SubUnitsForIUAsync(unitID, GetCreds(), request);
            return res;
        }

        public IObservable<IEnumerable<IdentificationUnitAnalysis>> GetAnalysesForIU(int unitID)
        {
            object request = new object();
            var res = from result in AnalysesForIUCompleted.MakeObservableServiceResultSingle(request)
                      select from an in result.Result
                             select an.ToClientObject();
            _svc.AnalysesForIUAsync(unitID, GetCreds(), request);
            return res;
        }
    }
}