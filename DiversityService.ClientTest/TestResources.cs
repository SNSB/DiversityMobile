using DiversityService.Test.ServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService.Test
{
    public static class TestResources
    {
        public static UserCredentials InitCredentials = new UserCredentials()
        {
            LoginName = "test_user",
            Password = "HinzKunz1"
        };

        public static UserCredentials Credentials = new UserCredentials()
        {
            LoginName = "test_user",
            Password = "HinzKunz1",
            Repository = "Bayreuth TestServer", //DisplayName!!
            AgentName = "test_user",
            AgentURI = "TestAgentURI"
        };

        public static int ProjectID = 1100;

        public static int SeriesID = -1;
        public static int EventID = 1;
        public static int SpecimenID = 1;
        public static int UnitID = 1;
    }
}
