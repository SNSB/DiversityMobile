using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService.Model
{
    public class UserCredentials
    {
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string Repository { get; set; }
        public string AgentName { get; set; }
        public string AgentURI { get; set; }
        public int ProjectID { get; set; }
    }
}
