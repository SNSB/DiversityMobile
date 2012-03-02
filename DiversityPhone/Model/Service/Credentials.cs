using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DiversityPhone.Model;

namespace DiversityPhone.DiversityService
{
    public partial class UserCredentials
    {
        public UserCredentials()
        {

        }

        public UserCredentials(AppSettings settings)
        {
            this.AgentName = settings.AgentName;
            this.AgentURI = settings.AgentURI;
            this.LoginName = settings.UserName;
            this.Password = settings.Password;
            this.ProjectID = settings.CurrentProject;
            this.Repository = settings.HomeDB;           
        }
    }
}
