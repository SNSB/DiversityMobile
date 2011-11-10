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
using System.Data.Linq.Mapping;

namespace DiversityPhone.Model
{
    [Table]
    public class UserProfile
    {

        [Column(IsPrimaryKey = true)]
        public String LoginName { get; set; }

        [Column]
        public String Password { get; set; }

        [Column]
        public int ProjectID { get; set; }

        [Column]
        public String AgentName { get; set; }

        [Column]
        public String AgentUri { get; set; }

        [Column]
        public bool RecordGeoPosition { get; set; }
    }
}
