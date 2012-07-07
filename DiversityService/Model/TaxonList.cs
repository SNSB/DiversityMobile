using System;
using PetaPoco;


namespace DiversityService.Model
{    
    [ExplicitColumns]
    public class TaxonList
    {
        [Column("DataSource")]
        public string Table { get; set; }
        [Column("TaxonomicGroup")]
        public string TaxonomicGroup { get; set; }
        [Column("DisplayText")]
        public string DisplayText { get; set; }
        
        public bool IsPublicList { get; set; }
    }
}
