using System.Linq;


namespace DiversityPhone.Model
{
    using System.Data.Linq.Mapping;

    [Table]
    public class TaxonName
    {
        [Column(IsPrimaryKey = true)]
        public string URI { get; set; }

        [Column]
        public string TaxonNameCache { get; set; }

        [Column]
        public string TaxonNameSinAuth { get; set; }

        [Column]
        public string GenusOrSupragenic { get; set; }

        [Column]
        public string SpeciesEpithet { get; set; }

        [Column]
        public string InfraspecificEpithet { get; set; }

        [Column]
        public Synonymy Synonymy { get; set; }

        [Column]
        public string AcceptedNameURI { get; set; }

        [Column]
        public string AcceptedNameCache { get; set; }

        public static IQueryOperations<TaxonName> Operations
        {
            get;
            private set;
        }

        static TaxonName()
        {
            Operations = new QueryOperations<TaxonName>(
                //Smallerthan
                          (q, es) => q.Where(row => row.URI.CompareTo(es.URI) <0),
                //Equals
                          (q, es) => q.Where(row => row.URI == es.URI),
                //Orderby
                          (q) => q.OrderBy(es => es.URI),
                //FreeKey
                          (q, es) =>
                          {
                              //Not Applicable
                          });
        }      
    }
}
