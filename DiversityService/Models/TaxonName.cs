using System;
using PetaPoco;


namespace DiversityService.Model
{
    [ExplicitColumns]    
    public class TaxonName
    {
        //Read-Only
        [Column("NameURI")]
        public virtual string URI { get; set; }
        [Column("TaxonNameCache")]
        public virtual string TaxonNameCache { get; set; }
        [Column("TaxonNameSinAuthors")]
        public virtual string TaxonNameSinAuth { get; set; }
        [Column("GenusOrSupragenericName")]
        public virtual string GenusOrSupragenic { get; set; }
        [Column("SpeciesEpithet")]
        public virtual string SpeciesEpithet { get; set; }
        [Column("InfraspecificEpithet")]
        public virtual string InfraspecificEpithet { get; set; }
        [Column("Synonymy")]
        public virtual string Synonymy { get; set; }
    }
}
