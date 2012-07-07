using System;
using PetaPoco;


namespace DiversityService.Model
{
    public class PropertyList
    {
        [Column("DataSource")]
        public string Table { get; set; }
        [Column("PropertyID")]
        public int PropertyID { get; set; }
    }
}
