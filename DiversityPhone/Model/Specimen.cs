namespace DiversityPhone.Model
{
    using System;
    using System.Linq;
    using System.Data.Linq.Mapping;
    using DiversityPhone.Services;

    [Table]
    public class Specimen : IModifyable
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


        public static IQueryOperations<Specimen> Operations
        {
            get;
            private set;
        }

        static Specimen()
        {
            Operations = new QueryOperations<Specimen>(
                //Smallerthan
                          (q, spec) => q.Where(row => row.CollectionSpecimenID < spec.CollectionSpecimenID),
                //Equals
                          (q, spec) => q.Where(row => row.CollectionSpecimenID == spec.CollectionSpecimenID),
                //Orderby
                          (q) => q.OrderBy(spec => spec.CollectionSpecimenID),
                //FreeKey
                          (q, spec) =>
                          {
                              spec.CollectionSpecimenID = QueryOperations<Specimen>.FindFreeIntKey(q, row => row.CollectionSpecimenID);
                          });
        }
    }
}
