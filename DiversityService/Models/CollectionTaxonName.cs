using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityMobileEntities
{
    public class CollectionTaxonName
    {
        public string NameURI { get; set; }
        public string TaxonNameCache { get; set; }
        public string TaxonNameSinAuthors { get; set; }
        public string GenusOrSupragenericName { get; set; }
        public string SpeciesEpithet { get; set; }
        public string InfraspecificEpithet { get; set; }
        public string Synonymy { get; set; }
        public string Family { get; set; }
        public string Order { get; set; }
    }
}
