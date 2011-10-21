using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService.Model
{
    public class UserProfile
    {
        public String LoginName { get; set; }
        public String Password { get; set; }
        public int ProjectID { get; set; }
        public String AgentName { get; set; }
        public String AgentUri { get; set; }
        public bool RecordGeoPosition { get; set; }

    }
}
