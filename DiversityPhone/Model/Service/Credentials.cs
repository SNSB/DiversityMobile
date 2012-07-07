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
    public static class UserCredentialsMixin
    {     

        public static UserCredentials ToCreds(this AppSettings settings)
        {
            return new UserCredentials(){
            AgentName = settings.AgentName,
            AgentURI = settings.AgentURI,
            LoginName = settings.UserName,
            Password = settings.Password,
            ProjectID = settings.CurrentProject,
            Repository = settings.HomeDB,
            };
        }
    }
}
