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
using System.Collections.Generic;
using System.Linq;

namespace DiversityPhone.Model
{
    [Table]    
    public class TaxonSelection
    {     
        [Column(IsPrimaryKey = true)]
        public int TableID;

        [Column]
        public string TableName;

        [Column]
        public string TableDisplayName;

        [Column]
        public string TaxonomicGroup;

        [Column]
        public bool IsSelected;


        public static IEnumerable<int> ValidTableIDs
        {
            get
            {
                return Enumerable.Range(0, 100);
            }
        }
    }
}
