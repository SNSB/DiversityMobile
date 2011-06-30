using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace LinqToSQL.Model
{
    [Table]
    public class StringISO
    {
        [Column]
        public string Value { get; set; }

        [Column(IsPrimaryKey=true)]
        public Guid GUID { get; set; }


    }
}
