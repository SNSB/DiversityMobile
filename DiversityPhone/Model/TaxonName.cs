using System;


namespace DiversityPhone.Model
{
    using System.Data.Linq.Mapping;
    using Microsoft.Phone.Data.Linq.Mapping;

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
        public string Synonymy { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }
      
    }
}
