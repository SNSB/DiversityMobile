namespace DiversityPhone.Services
{
    using System.Data.Linq;
    using DiversityPhone.Model;

    public class DiversityDataContext : DataContext
    {
        private static string connStr = "isostore:/diversityDB.sdf";

        public DiversityDataContext()
            : base(connStr)
        {
        }

        public Table<EventSeries> EventSeries;
        public Table<Event> Events;
        public Table<Specimen> Specimen;
        public Table<IdentificationUnit> IdentificationUnits;

        public Table<Term> Terms;
        public Table<TaxonName> TaxonNames;
    }
}
