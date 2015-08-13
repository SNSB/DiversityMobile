using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

using Service = DiversityPhone.DiversityService;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient : IDiversityServiceClient
    {
        public IObservable<IEnumerable<EventSeries>> GetEventSeriesByQuery(string query)
        {
            return DiversityServiceCall((Service.EventSeriesByQueryCompletedEventArgs args) => {
                return from series in args.Result
                       select series.ToClientObject();
            }, svc => {
                WithCredentials(c => svc.EventSeriesByQueryAsync(query, c));
            });
        }

        public IObservable<EventSeries> GetEventSeriesByID(int seriesID)
        {
            return DiversityServiceCall((Service.EventSeriesByIDCompletedEventArgs args) => {
                return args.Result.ToClientObject();
            }, svc => {
                WithCredentials(c => svc.EventSeriesByIDAsync(seriesID, c));
            });
        }

        public IObservable<IEnumerable<Event>> GetEventSeriesEvents(int seriesID)
        {
            return DiversityServiceCall((Service.EventsForSeriesCompletedEventArgs args) => {
                return from x in args.Result
                       select x.ToClientObject(seriesID);
            }, svc => {
                WithCredentials(c => svc.EventsForSeriesAsync(seriesID, c));
            });
        }

        public IObservable<IEnumerable<Localization>> GetEventSeriesLocalizations(int seriesID)
        {
            return DiversityServiceCall((Service.LocalizationsForSeriesCompletedEventArgs args) => {
                return from x in args.Result
                       select x.ToClientObject();
            }, svc => {
                WithCredentials(c => svc.LocalizationsForSeriesAsync(seriesID, c));
            });
        }

        public IObservable<IEnumerable<Event>> GetEventsByLocality(string localityQuery)
        {
            return DiversityServiceCall((Service.EventsByLocalityCompletedEventArgs args) => {
                return from x in args.Result
                       select x.ToClientObject(x.CollectionSeriesID);
            }, svc => {
                WithCredentials(c => svc.EventsByLocalityAsync(localityQuery, c));
            });
        }

        public IObservable<IEnumerable<EventProperty>> GetEventProperties(int eventID)
        {
            return DiversityServiceCall((Service.PropertiesForEventCompletedEventArgs args) => {
                return from x in args.Result
                       select x.ToClientObject();
            }, svc => {
                WithCredentials(c => svc.PropertiesForEventAsync(eventID, c));
            });
        }

        public IObservable<IEnumerable<Specimen>> GetSpecimenForEvent(int eventID)
        {
            return DiversityServiceCall((Service.SpecimenForEventCompletedEventArgs args) => {
                return from x in args.Result
                       select x.ToClientObject();
            }, svc => {
                WithCredentials(c => svc.SpecimenForEventAsync(eventID, c));
            });
        }

        public IObservable<IEnumerable<IdentificationUnit>> GetIdentificationUnitsForSpecimen(int specimenID)
        {
            return DiversityServiceCall((Service.UnitsForSpecimenCompletedEventArgs args) => {
                return from x in args.Result
                       select x.ToClientObject();
            }, svc => {
                WithCredentials(c => svc.UnitsForSpecimenAsync(specimenID, c));
            });
        }

        public IObservable<IEnumerable<IdentificationUnit>> GetSubUnitsForIU(int unitID)
        {
            return DiversityServiceCall((Service.SubUnitsForIUCompletedEventArgs args) => {
                return from x in args.Result
                       select x.ToClientObject();
            }, svc => {
                WithCredentials(c => svc.SubUnitsForIUAsync(unitID, c));
            });
        }

        public IObservable<IEnumerable<IdentificationUnitAnalysis>> GetAnalysesForIU(int unitID)
        {
            return DiversityServiceCall((Service.AnalysesForIUCompletedEventArgs args) => {
                return from x in args.Result
                       select x.ToClientObject();
            }, svc => {
                WithCredentials(c => svc.AnalysesForIUAsync(unitID, c));
            });
        }
    }
}