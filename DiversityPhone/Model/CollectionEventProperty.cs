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

namespace DiversityPhone.Model
{
    [Table]
    public class CollectionEventProperty
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
   

    }
}
