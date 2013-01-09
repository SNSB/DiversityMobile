﻿using DiversityORM;
using DiversityService.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DiversityService
{
    public partial class DiversityService
    {
        public int InsertEventSeries(EventSeries series, IEnumerable<Localization> localizations, UserCredentials login)
        {
            using (var db = new Diversity(login))
            {
                db.BeginTransaction();
                db.Insert(series);

                var geoString = SerializeLocalizations(localizations);
                if(!string.IsNullOrWhiteSpace(geoString))
                    db.Execute("Update [dbo].[CollectionEventSeries] Set geography=@0 Where SeriesID=@1", geoString, series.CollectionEventSeriesID);

                db.CompleteTransaction();
                return series.CollectionEventSeriesID;
            }
        }

        public int InsertEvent(Event ev, IEnumerable<EventProperty> properties, UserCredentials login)
        {
            using (var db = new Diversity(login))
            {
                db.BeginTransaction();                
                db.Insert(ev);

                var geoString = SerializeLocalization((double)ev.Latitude, (double)ev.Longitude, ev.Altitude);
                foreach (var loc in ev.GetLocalisations(login))
                {
                    db.Insert(loc);

                    if (!string.IsNullOrWhiteSpace(geoString))
                        db.Execute("Update [dbo].[CollectionEventLocalisation] Set geography=@0 Where CollectionEventID=@1 AND LocalisationSystemID=@2", geoString, loc.CollectionEventID, loc.LocalisationSystemID);                    
                }

                foreach (var p in properties)
                {
                    p.CollectionEventID = ev.CollectionEventID;
                    db.Insert(p);
                }


                db.CompleteTransaction();

                return ev.CollectionEventID;
            }
        }


        public int InsertSpecimen(Specimen s, UserCredentials login)
        {
            using (var db = new Diversity(login))
            {
                db.BeginTransaction();
                db.Insert(s);

                db.Insert(s.GetProject(login.ProjectID));
                db.Insert(s.GetAgent(login));

                db.CompleteTransaction();
                return s.CollectionSpecimenID;
            }
        }


        public int InsertIdentificationUnit(IdentificationUnit iu, IEnumerable<IdentificationUnitAnalysis> analyses, UserCredentials login)
        {
            using (var db = new Diversity(login))
            {
                db.BeginTransaction();
                db.Insert(iu);
                db.Insert(iu.GetIdentification(login));
                db.Insert(iu.GetGeoAnalysis(login));

                foreach (var a in analyses)
                {
                    a.CollectionUnitID = iu.CollectionUnitID;
                    a.CollectionSpecimenID = iu.CollectionSpecimenID;
                    db.Insert(a);
                }

                db.CompleteTransaction();

                return iu.CollectionUnitID;
            }
        }


        public void InsertMMO(MultimediaObject mmo, UserCredentials login)
        {
            using (var db = new DiversityORM.Diversity(login))
            {
                switch (mmo.OwnerType)
                {
                    case MultimediaOwner.EventSeries:
                        CollectionEventSeriesImage cesi = mmo.ToSeriesImage();
                        db.Insert(cesi);
                        break;
                    case MultimediaOwner.Event:
                        CollectionEventImage cei = mmo.ToEventImage();
                        db.Insert(cei);
                        break;
                    case MultimediaOwner.Specimen:
                    case MultimediaOwner.IdentificationUnit:
                        CollectionSpecimenImage csi = mmo.ToSpecimenImage(db);
                        db.Insert(csi);
                        break;
                    default:
                        throw new ArgumentException("unknown type");
                }
            }
        }

        private static String SerializeLocalizations(IEnumerable<Localization> locs)
        {
            if (locs.Any())
            {
                var cult = new CultureInfo("en-US");
                return string.Format("LINESTRING({0})",
                        string.Join(", ", locs.Select(gp => string.Format(cult, "{0} {1}", gp.Longitude, gp.Latitude)))
                    );
            }
            else return String.Empty;
        }

        private static String SerializeLocalization(double latitude, double longitude, double? altitude)
        {
            if (Double.IsNaN(latitude) || Double.IsNaN(longitude))
                return String.Empty;

            var cult = new CultureInfo("en-US");
            String longitudeStr = longitude.ToString(cult);

            String latStr = latitude.ToString(cult);
            latStr = latStr.Replace(',', '.');

            StringBuilder builder = new StringBuilder("POINT(");
            builder.Append(longitudeStr);
            builder.Append(" ");
            builder.Append(latStr);
            if (altitude.HasValue && Double.IsNaN((double)altitude) == false)
            {
                String altStr = altitude.Value.ToString(cult);
                altStr = altStr.Replace(',', '.');
                builder.Append(" ");
                builder.Append(altStr);
            }
            builder.Append(")");
            String s = builder.ToString();
            return builder.ToString();
        }
    }
}
