using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Data.Linq.Mapping;
using DiversityPhone.Services;

namespace DiversityPhone.Model
{
    [Table]
    public class CollectionEventProperty : IModifyable
    {
        public CollectionEventProperty()
        {
            LogUpdatedWhen = DateTime.Now;
            this.IsModified = null;
        }

        [Column(IsPrimaryKey = true)]
        public int EventID { get; set; }

        [Column(IsPrimaryKey = true)]
        public int PropertyID { get; set; }

        [Column]
        public String DisplayText { get; set; }

        [Column]
        public String PropertyUri { get; set; }




        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull = true)]
        public bool? IsModified { get; set; }
        [Column]
        public DateTime LogUpdatedWhen { get; set; }



        public static IQueryOperations<CollectionEventProperty> Operations
        {
            get;
            private set;
        }

        static CollectionEventProperty()
        {
            Operations = new QueryOperations<CollectionEventProperty>(
                //Smallerthan
                          (q, cep) => q.Where(row => row.EventID < cep.EventID || row.PropertyID < cep.PropertyID),
                //Equals
                          (q, cep) => q.Where(row => row.EventID == cep.EventID && row.PropertyID == cep.PropertyID),
                //Orderby
                          (q) => from cep in q
                                 orderby cep.EventID, cep.PropertyID
                                 select cep,
                //FreeKey
                          (q, cep) =>
                          {
                              //Not Applicable
                          });
        }
    }
}
