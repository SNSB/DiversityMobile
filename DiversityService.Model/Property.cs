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

namespace DiversityService.Model
{
    public class Property
    {
        //Read-Only

        public int PropertyID { get; set; }
        public string PropertyName { get; set; }
        public string DisplayText { get; set; }
        public string Description { get; set; }

        public DateTime LogUpdatedWhen { get; set; }
    }
}
