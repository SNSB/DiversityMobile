/*
 * Contains Model classes that will be sent to the client via WCF
 * 
 */
namespace DiversityPhone.Model
{
    public class Project
    {
        // Read-Only
        public int ProjectID { get; set; }

        public string DisplayText { get; set; }
    }

    public class UserProfile
    {
        public string LoginName { get; set; }

        public string UserName { get; set; }

        public string AgentUri { get; set; }
    }

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
