using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace DiversityService.TestClient
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            var svc = new ServiceReference1.DiversityServiceClient();
            svc.ClientCredentials.UserName.UserName = "Rollinger";
            svc.ClientCredentials.UserName.Password = "Rolli#2-AI4@UBT";

            svc.GetUserInfoCompleted += (s,args) => 
            { 
                System.Diagnostics.Debug.WriteLine(args.Result.AgentUri);
            };
            svc.GetUserInfoAsync();
        }
        
    }
}