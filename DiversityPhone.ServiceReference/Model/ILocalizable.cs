using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Model
{
    public interface ILocalizable
    {
        double? Altitude { get; set; }
        double? Latitude { get; set; }
        double? Longitude { get; set; }

        
    }
}
