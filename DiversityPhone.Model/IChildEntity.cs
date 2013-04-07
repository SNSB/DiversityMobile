using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Model
{
    public interface IChildEntity
    {
        DBObjectType ParentType { get; }
        int? ParentID { get; }
    }
}
