namespace DiversityService
{
    using DiversityPhone.Model;
    using Model;
    using System.Collections.Generic;

    /// <summary>
    /// Contains methods to convert the Client Objects that might differ in structure to their counterparts used in DiversityCollection
    /// </summary>
    public static class ClientServiceConversions
    {
        /// <summary>
        /// Extracts the geo location information stored in the client-side <see cref="Event"/> Object
        /// And converts it to its <see cref="CollectionEventLocalisation"/> representation.
        /// </summary>
        /// <param name="ev">The client-side Event object possibly containing location information.</param>
        /// <param name="profile">The Profile of the User responsible for creating this object.</param>        
        /// <returns>Between 0 and 2 <see cref="CollectionEventLocalisation"/> objects depending on the amount of information in the Input object.</returns>
        public static IEnumerable<CollectionEventLocalisation> GetLocalisations(this Event ev, UserCredentials profile)
        {
            IList<CollectionEventLocalisation> localisations = new List<CollectionEventLocalisation>();
            if (ev.Altitude.HasValue && !double.IsNaN(ev.Altitude.Value))
            {
                CollectionEventLocalisation altitude = new CollectionEventLocalisation();
                altitude.AverageAltitudeCache = ev.Altitude;
                altitude.AverageLatitudeCache = ev.Latitude;
                altitude.AverageLongitudeCache = ev.Longitude;
                altitude.CollectionEventID = ev.CollectionEventID;
                altitude.DeterminationDate = ev.CollectionDate;
                altitude.LocalisationSystemID = 4;
                altitude.Location1 = ev.Altitude.ToString();
                altitude.ResponsibleAgentURI = profile.AgentURI;
                altitude.ResponsibleName = profile.AgentName;
                altitude.RecordingMethod = "Generated via DiversityMobile";
                localisations.Add(altitude);
            }

            if (ev.Latitude.HasValue && !double.IsNaN(ev.Latitude.Value)
                && ev.Longitude.HasValue && !double.IsNaN(ev.Longitude.Value))
            {
                CollectionEventLocalisation wgs84 = new CollectionEventLocalisation();
                if (ev.Altitude != null && double.IsNaN((double)ev.Altitude) == false)
                {
                    wgs84.AverageAltitudeCache = ev.Altitude;
                }
                else
                {
                    wgs84.AverageAltitudeCache = null;
                }

                wgs84.AverageLatitudeCache = ev.Latitude;
                wgs84.AverageLongitudeCache = ev.Longitude;
                wgs84.CollectionEventID = ev.CollectionEventID;
                wgs84.DeterminationDate = ev.CollectionDate;
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

        public static CollectionProject GetProject(this Specimen s, int projectID)
        {
            return new CollectionProject()
            {
                CollectionSpecimenID = s.CollectionSpecimenID,
                ProjectID = projectID
            };
        }

        public static CollectionAgent GetAgent(this Specimen s, UserCredentials profile)
        {
            return new CollectionAgent()
            {
                CollectionSpecimenID = s.CollectionSpecimenID,
                CollectorsAgentURI = profile.AgentURI,
                CollectorsName = profile.AgentName
            };
        }

        public static Identification GetIdentification(this IdentificationUnit iu, UserCredentials profile)
        {
            return new Identification()
            {
                CollectionSpecimenID = iu.CollectionSpecimenID,
                IdentificationUnitID = iu.CollectionUnitID,
                IdentificationSequence = 1,
                IdentificationDay = (byte?)iu.AnalysisDate.Day,
                IdentificationMonth = (byte?)iu.AnalysisDate.Month,
                IdentificationYear = (short?)iu.AnalysisDate.Year,
                IdentificationDateCategory = "actual",
                TaxonomicName = iu.LastIdentificationCache,
                NameURI = iu.IdentificationUri,
                IdentificationCategory = "determination",
                ResponsibleName = profile.AgentName,
                ResponsibleAgentURI = profile.AgentURI,
                IdentificationQualifier = iu.Qualification
            };
        }

        public static IdentificationUnitGeoAnalysis GetGeoAnalysis(this IdentificationUnit iu, UserCredentials profile)
        {
            return new IdentificationUnitGeoAnalysis()
            {
                AnalysisDate = iu.AnalysisDate,
                IdentificationUnitID = iu.CollectionUnitID,
                CollectionSpecimenID = iu.CollectionSpecimenID,
                ResponsibleName = profile.AgentName,
                ResponsibleAgentURI = profile.AgentURI,
            };
        }
    }
}
