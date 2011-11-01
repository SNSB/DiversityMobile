using System;


namespace DiversityService.Model
{
    public class TaxonName
    {
        //Read-Only

        public virtual string URI { get; set; }
        public virtual string TaxonNameCache { get; set; }
        public virtual string TaxonNameSinAuth { get; set; }
        public virtual string GenusOrSupragenic { get; set; }
        public virtual string SpeciesEpithet { get; set; }
        public virtual string InfraspecificEpithet { get; set; }
        public virtual string Synonymy { get; set; }
    }
}
