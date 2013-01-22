
using System.Collections.Generic;
using Client = DiversityPhone.Model;
using System;
using System.Reactive.Linq;
using DiversityPhone.DiversityService;
using DiversityPhone.MultimediaService;
using System.Linq;
using ReactiveUI;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient : IDiversityServiceClient
    {


        public IObservable<Client.EventSeries> GetEventSeriesByID(int seriesID)
        {
            throw new NotImplementedException();
        }

        public IObservable<IEnumerable<Client.Localization>> GetEventSeriesLocalizations(int seriesID)
        {
            throw new NotImplementedException();
        }

        public IObservable<IEnumerable<Client.Event>> GetEventsByLocality(string localityQuery)
        {
            throw new NotImplementedException();
        }

        public IObservable<IEnumerable<Client.EventProperty>> GetEventProperties(int eventID)
        {
            throw new NotImplementedException();
        }

        public IObservable<IEnumerable<Client.Specimen>> GetSpecimenForEvent(int eventID)
        {
            throw new NotImplementedException();
        }

        public IObservable<IEnumerable<Client.IdentificationUnit>> GetIdentificationUnitsForSpecimen(int specimenID)
        {
            throw new NotImplementedException();
        }

        public IObservable<IEnumerable<Client.IdentificationUnit>> GetSubUnitsForIU(int unitID)
        {
            throw new NotImplementedException();
        }

        public IObservable<IEnumerable<Client.IdentificationUnitAnalysis>> GetAnalysesForIU(int unitID)
        {
            throw new NotImplementedException();
        }
    }
}
