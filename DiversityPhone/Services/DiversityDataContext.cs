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
        public Table<MultimediaObject> CollectionEventImages;
        public Table<CollectionEventProperty> CollectionEventProperties;
        public Table<Term> Properties;
       
        
        
        public Table<Specimen> Specimen;
        public Table<MultimediaObject> CollectionSpecimenImages;

        public Table<IdentificationUnit> IdentificationUnits;
        public Table<Term> TaxonomicGroups;
        public Table<Term> UnitRelationType;

        public Table<MultimediaObject> IdentificationUnitImages;


        public Table<IdentificationUnitAnalysis> IdentificationUnitAnalyses;
        public Table<Analysis> Analyses;
        public Table<Term> AnalysisResults;
        public Table<Term> AnalysisTaxonomicGroups;

        public Table<Term> Terms;
        public Table<TaxonName> TaxonNames;
    }
}
