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
using Svc = DiversityPhone.DiversityService;

namespace DiversityPhone.Model
{
    [Table]
    public class MultimediaObject : IModifyable
    {
        [Column]
        public ReferrerType OwnerType { get; set; }

        [Column]
        public int RelatedId { get; set; }

        [Column(CanBeNull = true)]
        public int? DiversityCollectionRelatedID { get; set; }

        [Column(IsPrimaryKey = true)]
        public String Uri { get; set; }

        [Column]
        public MediaType MediaType { get; set; }

       
        /// <summary>
        /// Tracks modifications to this Object.        
        /// </summary>
        [Column]
        public ModificationState ModificationState { get; set; }
        
        [Column]
        public DateTime LogUpdatedWhen { get; set; }

        public static IQueryOperations<MultimediaObject> Operations
        {
            get;
            private set;
        }

        static MultimediaObject()
        {
            Operations = new QueryOperations<MultimediaObject>(
                //Smallerthan
                         (q, mmo) => q.Where(row => row.RelatedId < mmo.RelatedId),
                //Equals
                         (q, mmo) => q.Where(row => row.Uri == mmo.Uri),
                //Orderby
                         (q) => from mmo in q
                                orderby mmo.RelatedId, mmo.OwnerType
                                select mmo,
                //FreeKey
                         (q, cep) =>
                         {
                             //Not Applicable
                         });
        }


    }
}
