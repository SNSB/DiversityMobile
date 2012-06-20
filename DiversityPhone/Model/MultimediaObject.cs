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
        
        [Column(IsPrimaryKey = true)]
        public int MMOID { get; set; }

        [Column]
        public ReferrerType OwnerType { get; set; }

        [Column]
        public int RelatedId { get; set; }

        [Column(CanBeNull = true)]
        public int? DiversityCollectionRelatedID { get; set; }

        [Column]
        public String Uri { get; set; }

        [Column(CanBeNull = true)]
        public String DiversityCollectionUri;

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
                         (q, mmo) => q.Where(row => row.MMOID < mmo.MMOID),
                //Equals
                         (q, mmo) => q.Where(row => row.MMOID == mmo.MMOID),
                //Orderby
                         (q) => from mmo in q
                                orderby mmo.MediaType, mmo.OwnerType, mmo.RelatedId
                                select mmo,
                //FreeKey
                         (q, mmo) =>
                         {
                             mmo.MMOID = QueryOperations<MultimediaObject>.FindFreeIntKey(q, row => row.MMOID);
                         });
        }

        public static Svc.MultimediaObject ToServiceObject(MultimediaObject mmo)
        {
            if (mmo.DiversityCollectionRelatedID == null)
                throw new Exception("Partner not synced");
            Svc.MultimediaObject export = new Svc.MultimediaObject();
            export.LogUpdatedWhen = mmo.LogUpdatedWhen;
            export.MediaType = mmo.MediaType.ToString().ToLower();
            export.OwnerType = mmo.OwnerType.ToString();
            export.RelatedId = (int) mmo.DiversityCollectionRelatedID;
            export.Uri = mmo.DiversityCollectionUri;
            return export;
        }
        
    }
}
