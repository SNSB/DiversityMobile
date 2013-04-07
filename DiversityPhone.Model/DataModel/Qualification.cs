namespace DiversityPhone.Model
{
    using System.Data.Linq.Mapping;

    [Table]
    public class Qualification
    {
        [Column(IsPrimaryKey=true)]
        public string Code { get; set; }
        [Column]
        public string DisplayText { get; set; }
    }
}
