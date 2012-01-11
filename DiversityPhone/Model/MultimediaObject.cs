using System;
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
    public class MultimediaObject
    {
        [Column]
        public ReferrerType OwnerType { get; set; }

        [Column]
        public int RelatedId { get; set; }

        [Column(IsPrimaryKey = true)]
        public String Uri { get; set; }

        [Column]
        public String MediaType { get; set; }

       
        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull = true)]
        public bool? IsModified { get; set; }
        
        [Column]
        public DateTime LogUpdatedWhen { get; set; }
    }
}
