namespace DiversityPhone.Model
{
    using System;

    internal static class NoEventSeriesMixin
    {
        public static EventSeries NoEventSeries
        {
            get;
            private set;
        }

        public static bool IsNoEventSeries(this EventSeries This)
        {
            return This == NoEventSeries;
        }

        static NoEventSeriesMixin()
        {
            NoEventSeries = new EventSeries()
            {
                CollectionSeriesID = null,
                Description = DiversityResources.EventSeries_NoES_Header,
                ModificationState = ModificationState.Unmodified,
                SeriesCode = string.Empty,
                SeriesStart = DateTime.MinValue,
                SeriesEnd = DateTime.MinValue
            };
        }

        public static int? EventSeriesID(this EventSeries This)
        {
            if (This.IsNoEventSeries())
                return null;
            else
                return This.SeriesID;
        }
    }
}