using DiversityPhone.Model;
using System.Linq;

namespace DiversityPhone.Services
{
    static class Queries
    {
        internal static IQueryable<MultimediaObject> Multimedia(IMultimediaOwner owner, DiversityDataContext ctx)
        {
            return from mmo in ctx.MultimediaObjects
                   where mmo.OwnerType == owner.EntityType && mmo.RelatedId == owner.EntityID
                   select mmo;
        }

        internal static IQueryable<Event> Events(EventSeries es, DiversityDataContext ctx)
        {
            return from ev in ctx.Events
                   where ev.SeriesID == es.SeriesID
                   select ev;
        }

        internal static IQueryable<GeoPointForSeries> GeoPoints(EventSeries es, DiversityDataContext ctx)
        {
            return from gp in ctx.GeoTour
                   where gp.SeriesID == es.SeriesID
                   select gp;
        }

        internal static IQueryable<Specimen> Specimen(Event ev, DiversityDataContext ctx)
        {
            return from s in ctx.Specimen
                   where s.EventID == ev.EventID
                   select s;
        }

        internal static IQueryable<EventProperty> Properties(Event ev, DiversityDataContext ctx)
        {
            return from p in ctx.EventProperties
                   where p.EventID == ev.EventID
                   select p;
        }

        internal static IQueryable<IdentificationUnit> Units(Specimen s, DiversityDataContext ctx)
        {
            return from iu in ctx.IdentificationUnits
                   where iu.SpecimenID == s.SpecimenID
                   select iu;
        }

        internal static IQueryable<IdentificationUnit> SubUnits(IdentificationUnit u, DiversityDataContext ctx)
        {
            return from iu in ctx.IdentificationUnits
                   where iu.RelatedUnitID == u.UnitID
                   select iu;
        }

        internal static IQueryable<IdentificationUnitAnalysis> Analyses(IdentificationUnit iu, DiversityDataContext ctx)
        {
            return from a in ctx.IdentificationUnitAnalyses
                   where a.UnitID == iu.UnitID
                   select a;
        }
    }

}