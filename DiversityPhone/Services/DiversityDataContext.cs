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
using System.Data.Linq;
using DiversityService.Model;

namespace DiversityPhone.Services
{
    public class DiversityDataContext : DataContext
    {
        public DiversityDataContext()
            : base("isostore:/diversityDB.sdf")
        {

        }

        public Table<EventSeries> EventSeries;
        public Table<Event> Events;
        public Table<IdentificationUnit> IdentificationUnits;
    }
}
