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

namespace DiversityPhone.Services
{
    public class BackgroundTaskUpdate
    {
        public const BackgroundTaskUpdate Finished = new BackgroundTaskUpdate()
        {
            ProgressPercentage = 100,
            StatusMessage = null           
        };      


        public int ProgressPercentage { get; set; }
        public string StatusMessage { get; set; }
    }
}
