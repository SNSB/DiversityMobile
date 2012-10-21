using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiversityService.Model;

namespace DiversityService
{
    public static class PetaPocoProjection
    {
        public static IList<CollectionEventLocalisation> ToLocalisations(this Event ev, UserCredentials profile)
        {
            IList<CollectionEventLocalisation> localisations = new List<CollectionEventLocalisation>();
            if (ev.Altitude != null && Double.IsNaN((double) ev.Altitude)==false)
            {
                CollectionEventLocalisation altitude = new CollectionEventLocalisation();
                altitude.AverageAltitudeCache = ev.Altitude;
                altitude.AverageLatitudeCache = ev.Latitude;
                altitude.AverageLongitudeCache = ev.Longitude;
                altitude.CollectionEventID =(int) ev.DiversityCollectionEventID;
                altitude.DeterminationDate = ev.DeterminationDate;
                altitude.LocalisationSystemID = 4;
                altitude.Location1 = ev.Altitude.ToString();
                altitude.ResponsibleAgentURI = profile.AgentURI;
                altitude.ResponsibleName = profile.AgentName;
                altitude.RecordingMethod = "Generated via DiversityMobile";
                localisations.Add(altitude);
            }
            if (ev.Latitude != null && Double.IsNaN((double)ev.Latitude) == false && ev.Longitude != null && Double.IsNaN((double)ev.Longitude) == false)
            {
                CollectionEventLocalisation wgs84 = new CollectionEventLocalisation();
                if (ev.Altitude!=null && Double.IsNaN((double)ev.Altitude) == false)
                    wgs84.AverageAltitudeCache = ev.Altitude;
                else 
                    wgs84.AverageAltitudeCache = null;
                wgs84.AverageLatitudeCache = ev.Latitude;
                wgs84.AverageLongitudeCache = ev.Longitude;
                wgs84.CollectionEventID = (int)ev.DiversityCollectionEventID;
                wgs84.DeterminationDate = ev.DeterminationDate;
                wgs84.LocalisationSystemID = 8;
                wgs84.Location1 = ev.Longitude.ToString();
                wgs84.Location2 = ev.Latitude.ToString();
                wgs84.ResponsibleAgentURI = profile.AgentURI;
                wgs84.ResponsibleName = profile.AgentName;
                wgs84.RecordingMethod = "Generated via DiversityMobile";
                localisations.Add(wgs84);
            }
            return localisations;
        }

        public static CollectionProject ToProject(int specimenID, int projectID)
        {
            CollectionProject export = new CollectionProject();
            export.CollectionSpecimenID = specimenID;
            export.ProjectID = projectID;
            return export;
        }

        public static CollectionAgent ToAgent(int specimenID, Model.UserCredentials profile)
        {
            CollectionAgent export = new CollectionAgent();
            export.CollectionSpecimenID = specimenID;
            export.CollectorsAgentURI = profile.AgentURI;
            export.CollectorsName = profile.AgentName;
            return export;
        }

        public static Identification ToIdentification(this IdentificationUnit iu, UserCredentials profile)
        {
            if (iu.DiversityCollectionSpecimenID == null)
                throw new KeyNotFoundException();
            Identification export = new Identification();
            export.CollectionSpecimenID = (int)iu.DiversityCollectionSpecimenID;
            export.IdentificationUnitID = (int)iu.DiversityCollectionUnitID;
            export.IdentificationSequence = 1;
            export.IdentificationDay = (byte?)iu.LogUpdatedWhen.Day;
            export.IdentificationMonth = (byte?)iu.LogUpdatedWhen.Month;
            export.IdentificationYear = (byte?)iu.LogUpdatedWhen.Year;
            export.IdentificationDateCategory = "actual";
            export.TaxonomicName = iu.LastIdentificationCache;
            export.NameURI = iu.IdentificationUri;
            export.IdentificationCategory = "determination";
            export.ResponsibleName = profile.AgentName;
            export.ResponsibleAgentURI = profile.AgentURI;
            export.IdentificationQualifier = iu.Qualification;
            return export;
        }

        public static IdentificationUnitGeoAnalysis ToGeoAnalysis(this IdentificationUnit iu, UserCredentials profile)
        {
            if (iu.DiversityCollectionSpecimenID == null)
                throw new KeyNotFoundException();
            IdentificationUnitGeoAnalysis export = new IdentificationUnitGeoAnalysis();
            export.AnalysisDate = iu.AnalysisDate;
            export.IdentificationUnitID = (int)iu.DiversityCollectionUnitID;
            export.CollectionSpecimenID = (int)iu.DiversityCollectionSpecimenID;
            export.ResponsibleName = profile.AgentName;
            export.ResponsibleAgentURI = profile.AgentURI;
            return export;
        }

    }
}
