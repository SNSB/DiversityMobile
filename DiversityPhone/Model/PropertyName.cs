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
    public class PropertyName
    {
         //Read-Only
        [Column(IsPrimaryKey = true)]
        public string PropertyUri { get; set; }

        [Column]
        public int PropertyID { get; set; }      

        [Column]
        public string DisplayText { get; set; } 
    }
}
