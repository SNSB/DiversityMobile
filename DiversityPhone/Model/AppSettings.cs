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

namespace DiversityPhone.Model
{
    public class AppSettings
    {
        public AppSettings()
        {
            UseGPS = true;
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string AgentName { get; set; }
        public string AgentURI { get; set; }        
        public bool UseGPS { get; set; }

        public string HomeDB { get; set; }
        public string HomeDBName { get; set; }

        public int CurrentProject { get; set; }
        public string CurrentProjectName { get; set; }

        public AppSettings Clone()
        {
            return (AppSettings)this.MemberwiseClone();
        }       
            
    }

    
}
