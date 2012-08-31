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
    public class TaxonList : IEquatable<TaxonList>
    {
        public TaxonList()
        {
            TableID = -1;
        }

        [Column(IsPrimaryKey = true)]
        public int TableID { get; set; }

        [Column]
        public string TableName { get; set; }

        [Column]
        public string TableDisplayName { get; set; }

        [Column]
        public string TaxonomicGroup { get; set; }

        [Column]
        public bool IsPublicList { get; set; }

        [Column]
        public bool IsSelected { get; set; }


        public static IEnumerable<int> ValidTableIDs
        {
            get
            {
                return Enumerable.Range(0, 100);
            }
        }

        public bool Equals(TaxonList other)
        {
            return this.TableName == other.TableName && this.TaxonomicGroup == other.TaxonomicGroup;
        }
    }
}
