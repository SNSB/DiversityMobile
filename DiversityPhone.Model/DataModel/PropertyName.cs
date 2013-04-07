using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq.Mapping;

namespace DiversityPhone.Model
{
    [Table]
#if !TEST
    [Index(Columns="DisplayText", IsUnique=false, Name="text_idx")]
#endif
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
