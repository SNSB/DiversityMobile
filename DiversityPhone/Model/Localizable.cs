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
using ReactiveUI;

namespace DiversityPhone.Model
{
    public class Localizable:ReactiveObject, ILocalizable
    {
        private double? _Altitude;
        public double? Altitude
        {
            get { return _Altitude; }
            set { this.RaiseAndSetIfChanged(x => x.Altitude, ref _Altitude, value); }
        }
        private double? _Latitude;
        public double? Latitude
        {
            get { return _Latitude; }
            set { this.RaiseAndSetIfChanged(x => x.Latitude, ref _Latitude, value); }
        }
        private double? _Longitude;
        public double? Longitude
        {
            get { return _Longitude; }
            set { this.RaiseAndSetIfChanged(x => x.Longitude, ref _Longitude, value); }
        }
    }
}
