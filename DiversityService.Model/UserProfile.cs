using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService.Model
{
    public class UserProfile
    {
        public string LoginName { get; set; }
        public string Password { get; set; }
        public int ProjectID { get; set; }
        public string AgentName { get; set; }

    }
}
