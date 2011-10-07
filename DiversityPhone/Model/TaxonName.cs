namespace DiversityPhone.Model
{
    using System.Data.Linq.Mapping;
    using Microsoft.Phone.Data.Linq.Mapping;

    [Table]
    [Index(Columns = "TaxonomicGroup", IsUnique = false, Name = "taxonname_group")]
    public class TaxonName
    {
        [Column(IsPrimaryKey = true)]
        public string URI { get; set; }

        [Column]
        public string TaxonomicGroup { get; set; }

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
    }
}
