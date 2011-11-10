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

       
        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull = true)]
        public bool? IsModified { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }


         public Specimen()
        {
            this.AccesionNumber = null;
            this.LogUpdatedWhen = DateTime.Now;
            this.IsModified = null;
        }

    }
}
