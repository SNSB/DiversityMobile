using System;
using PetaPoco;

namespace DiversityService.Model
{
    [TableName("CollectionSpecimen")]
    [PrimaryKey("CollectionSpecimenID", autoIncrement = true)]
    public class Specimen
    {
        [Ignore]
        public int CollectionSpecimenID{get; set;}
        [Column("CollectionSpecimenID")]
        public int DiversityCollectionSpecimenID { get; set; }

        [Ignore]
        public int CollectionEventID { get; set; }

        [Column("CollectionEventID")]
        public int? DiversityCollectionEventID { get; set; }
        [Column("DepositorsAccessionNumber")]
        public string AccessionNumber { get; set; }

        [Ignore]
        public DateTime LogUpdatedWhen { get; set; }
    }
}
