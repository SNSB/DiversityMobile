namespace DiversityPhone.Model
{
    using DiversityPhone.Interface;
    using System;
    using System.Collections.Generic;
    using Svc = DiversityPhone.DiversityService;

    public static class ServiceProjectionExtensions
    {
        public static Svc.EventSeries ToServiceObject(this EventSeries es)
        {
            if (es.CollectionSeriesID.HasValue)
                throw new ArgumentException("Series already uploaded");

            return new Svc.EventSeries()
            {
                CollectionEventSeriesID = Int32.MinValue,
                SeriesCode = es.SeriesCode,
                SeriesStart = es.SeriesStart,
                SeriesEnd = es.SeriesEnd,
                Description = es.Description
            };
        }

        public static EventSeries ToClientObject(this Svc.EventSeries es)
        {
            return new EventSeries()
            {
                CollectionSeriesID = es.CollectionEventSeriesID,
                SeriesCode = es.SeriesCode,
                SeriesStart = es.SeriesStart ?? DateTime.MinValue,
                SeriesEnd = es.SeriesEnd,
                Description = es.Description,
                ModificationState = ModificationState.Unmodified,
            };
        }

        public static Svc.Localization ToServiceObject(this ILocalizable l)
        {
            return new Svc.Localization()
            {
                Altitude = l.Altitude,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
            };
        }

        public static Localization ToClientObject(this Svc.Localization l)
        {
            return new Localization()
            {
                Altitude = l.Altitude,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                ModificationState = ModificationState.Unmodified,
            };
        }

        public static Svc.Event ToServiceObject(this Event ev, IKeyMappingService mapping)
        {
            if (ev.CollectionEventID != null)
                throw new ArgumentException("Event already uploaded");

            var seriesID = (ev.SeriesID.HasValue) ? mapping.EnsureKey(DBObjectType.EventSeries, ev.SeriesID.Value) as int? : null;

            return new Svc.Event()
            {
                CollectionEventID = Int32.MinValue,
                CollectionSeriesID = seriesID,
                Altitude = ev.Altitude,
                CollectionDate = ev.CollectionDate,
                HabitatDescription = ev.HabitatDescription,
                Latitude = ev.Latitude,
                LocalityDescription = ev.LocalityDescription,
                Longitude = ev.Longitude,
            };
        }

        public static Event ToClientObject(this Svc.Event ev, int? SeriesID)
        {
            return new Event()
            {
                CollectionEventID = ev.CollectionEventID,
                SeriesID = SeriesID,
                CollectionDate = ev.CollectionDate ?? DateTime.Now,
                HabitatDescription = ev.HabitatDescription,
                LocalityDescription = ev.LocalityDescription,
                Altitude = ev.Altitude,
                Latitude = ev.Latitude,
                Longitude = ev.Longitude,
                ModificationState = ModificationState.Unmodified,
            };
        }

        public static Svc.EventProperty ToServiceObject(this EventProperty cep)
        {
            return new Svc.EventProperty()
            {
                DisplayText = cep.DisplayText,
                PropertyID = cep.PropertyID,
                PropertyUri = cep.PropertyUri,
            };
        }

        public static EventProperty ToClientObject(this Svc.EventProperty cep)
        {
            return new EventProperty()
            {
                DisplayText = cep.DisplayText,
                PropertyID = cep.PropertyID,
                PropertyUri = cep.PropertyUri,
                ModificationState = ModificationState.Unmodified,
            };
        }

        public static Svc.Specimen ToServiceObject(this Specimen spec, IKeyMappingService mapping)
        {
            var eventID = mapping.EnsureKey(DBObjectType.Event, spec.EventID);

            return new Svc.Specimen()
            {
                CollectionSpecimenID = Int32.MinValue,
                CollectionEventID = eventID,
                AccessionNumber = spec.AccessionNumber,
            };
        }

        public static Specimen ToClientObject(this Svc.Specimen spec)
        {
            return new Specimen()
            {
                AccessionNumber = spec.AccessionNumber,
                CollectionSpecimenID = spec.CollectionSpecimenID,
                ModificationState = ModificationState.Unmodified,
            };
        }

        public static Svc.IdentificationUnit ToServiceObject(this IdentificationUnit iu, IKeyMappingService mapping)
        {
            var specID = mapping.EnsureKey(DBObjectType.Specimen, iu.SpecimenID);

            var relUnitID = (iu.RelatedUnitID.HasValue) ? mapping.EnsureKey(DBObjectType.IdentificationUnit, iu.RelatedUnitID.Value) as int? : null;

            return new Svc.IdentificationUnit()
            {
                CollectionUnitID = Int32.MinValue,
                CollectionSpecimenID = specID,
                CollectionRelatedUnitID = relUnitID,
                Altitude = iu.Altitude,
                AnalysisDate = iu.AnalysisDate,
                IdentificationUri = iu.IdentificationUri,
                LastIdentificationCache = iu.WorkingName,
                Qualification = iu.Qualification,
                Latitude = iu.Latitude,
                Longitude = iu.Longitude,
                OnlyObserved = iu.OnlyObserved,
                RelationType = iu.RelationType,
                TaxonomicGroup = iu.TaxonomicGroup,
            };

        }

        public static IdentificationUnit ToClientObject(this Svc.IdentificationUnit iu)
        {
            return new IdentificationUnit()
            {
                Altitude = iu.Altitude,
                AnalysisDate = iu.AnalysisDate,
                CollectionUnitID = iu.CollectionUnitID,
                RelatedUnitID = iu.CollectionRelatedUnitID,
                IdentificationUri = iu.IdentificationUri,
                Latitude = iu.Latitude,
                Longitude = iu.Longitude,
                OnlyObserved = iu.OnlyObserved,
                Qualification = iu.Qualification,
                RelationType = iu.RelationType,
                TaxonomicGroup = iu.TaxonomicGroup,
                WorkingName = iu.LastIdentificationCache,
                ModificationState = ModificationState.Unmodified,
            };
                
        }

        public static Svc.IdentificationUnitAnalysis ToServiceObject(this IdentificationUnitAnalysis iua)
        {
            return new Svc.IdentificationUnitAnalysis()
            {
                AnalysisDate = iua.AnalysisDate,
                AnalysisID = iua.AnalysisID,
                AnalysisResult = iua.AnalysisResult,
            };
        }

        public static IdentificationUnitAnalysis ToClientObject(this Svc.IdentificationUnitAnalysis iua)
        {
            return new IdentificationUnitAnalysis()
            {
                AnalysisDate = iua.AnalysisDate,
                AnalysisID = iua.AnalysisID,
                AnalysisResult = iua.AnalysisResult,
                ModificationState = ModificationState.Unmodified,
            };
        }



        public static Svc.MultimediaObject ToServiceObject(this MultimediaObject mmo, IKeyMappingService mapping)
        {
            var relID = mapping.EnsureKey(mmo.OwnerType, mmo.RelatedId);

            return new Svc.MultimediaObject()
            {
                MediaType = mmo.MediaType.ToServiceObject(),
                OwnerType = mmo.OwnerType.ToServiceObject(),
                RelatedCollectionID = relID,
                Uri = mmo.CollectionUri
            };

        }

        private static Svc.MultimediaType ToServiceObject(this MediaType t)
        {
            switch (t)
            {
                case MediaType.Image:
                    return Svc.MultimediaType.Image;
                case MediaType.Audio:
                    return Svc.MultimediaType.Audio;
                case MediaType.Video:
                    return Svc.MultimediaType.Video;
                default:
                    throw new NotImplementedException();
            }
        }

        private static Svc.MultimediaOwner ToServiceObject(this DBObjectType t)
        {
            switch (t)
            {

                case DBObjectType.EventSeries:
                    return Svc.MultimediaOwner.EventSeries;
                case DBObjectType.Event:
                    return Svc.MultimediaOwner.Event;
                case DBObjectType.Specimen:
                    return Svc.MultimediaOwner.Specimen;
                case DBObjectType.IdentificationUnit:
                    return Svc.MultimediaOwner.IdentificationUnit;
                default:
                    throw new ArgumentException();
            }
        }
    }
}