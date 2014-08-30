using System.Data.Linq.Mapping;

namespace DiversityPhone.Model
{
    [Table]
    public class Property
    {
        //Read-Only
        [Column(IsPrimaryKey = true)]
        public int PropertyID { get; set; }

        [Column]
        public string DisplayText { get; set; }
    }
}