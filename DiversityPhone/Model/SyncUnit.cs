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
using System.Collections.Generic;
using System.Linq;

namespace DiversityPhone.Model
{
    public class SyncUnit
    {
        public int? SeriesID { get; private set; }
        public int EventID { get; private set; }
        public List<int> SpecimenIDs { get; private set; }
        public List<int> UnitIDs { get; private set; }
        public List<int> AnalysisIDs { get; private set; }

        public SyncUnit(int? series, int ev)
        {
            SeriesID = series;
            EventID = ev;
            SpecimenIDs = new List<int>();
            UnitIDs = new List<int>();
            AnalysisIDs = new List<int>();
        }

        public void increment(SyncUnit inc)
        {            
            (SpecimenIDs as List<int>).AddRange(inc.SpecimenIDs);
            (UnitIDs as List<int>).AddRange(inc.UnitIDs);
            (AnalysisIDs as List<int>).AddRange(inc.AnalysisIDs);
        }

        public int Size 
        { 
            get
            {               
                return SpecimenIDs.Count + UnitIDs.Count + AnalysisIDs.Count;                
            }
        }       

    }
}
