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
using System.Data.Linq;
using System.Linq;
using DiversityPhone.Services;


namespace DiversityPhone.Model
{
    [Table]
    public class Analysis
    {
        //Read-Only

        public Analysis()
        {
            LogUpdatedWhen = DateTime.Now;
        }

        [Column(IsPrimaryKey = true)]
        public int AnalysisID { get; set; }

        [Column]
        public String DisplayText { get; set; }

        [Column]
        public String Description { get; set; }

        [Column]
        public String MeasurementUnit { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }


        public static IQueryOperations<Analysis> Operations
        {
            get;
            private set;
        }

        static Analysis()
        {
            Operations = new QueryOperations<Analysis>(
                //Smallerthan
                          (q, an) => q.Where(row => row.AnalysisID < an.AnalysisID),
                //Equals
                          (q, an) => q.Where(row => row.AnalysisID == an.AnalysisID),
                //Orderby
                          (q) => q.OrderBy(an => an.AnalysisID),
                //FreeKey
                          (q, an) =>
                          {
                              an.AnalysisID = QueryOperations<Analysis>.FindFreeIntKey(q, row => row.AnalysisID);
                          });
        }
    }
}
