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
using DiversityPhone.Model;
using System.Data.Linq.Mapping;
using System.IO;


namespace DiversityPhone.Services
{
    public class DiversityDataContext : DataContext
    {       
        static string connStr = "isostore:/diversityDB.sdf";        

        public DiversityDataContext()
            :base(connStr)
        {           
        }       

        public Table<EventSeries> EventSeries;
        public Table<Event> Events;
        public Table<IdentificationUnit> IdentificationUnits;

        public Table<Term> Terms;
        public Table<TaxonName> TaxonNames;
        
    }
}
