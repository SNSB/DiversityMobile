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

namespace SterlingToLINQ.Sterling
{
    public class Row
    {
        public Row()
        {
            GUID = Guid.NewGuid();
        }
        public string Value { get; set; }
        public Guid GUID { get; set; }
    }
}
