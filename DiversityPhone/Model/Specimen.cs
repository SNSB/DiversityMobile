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
using Microsoft.Phone.Data.Linq.Mapping;

namespace DiversityPhone.Model
{
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
