namespace DiversityPhone.Model
{
    using System;
    using System.Data.Linq.Mapping;

    [Table]
    public class Specimen
    {
        [Column(IsPrimaryKey = true)]
        public int CollectionSpecimenID { get; set; }

        [Column]
        public int CollectionEventID { get; set; }

        [Column]
        public string AccesionNumber { get; set; }

        [Column]
        public DateTime AccessionDate { get; set; }

        [Column]
        public bool? IsModified { get; set; }
    }
}
