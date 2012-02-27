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
using System.Linq;
using DiversityPhone.Services;


namespace DiversityPhone.Model
{
    [Table]
    public class AnalysisResult
    {

        public AnalysisResult()
        {
            //LogUpdatedWhen = DateTime.Now;
        }

        //Read-Only
        [Column(IsPrimaryKey = true)]
        public int AnalysisID { get; set; }

        [Column(IsPrimaryKey = true)]
        public string Result { get; set; }

        [Column]
        public string Description { get; set; }

        [Column]
        public string Notes { get; set; }

        [Column]
        public string DisplayText { get; set; }

        //[Column]
        //public DateTime LogUpdatedWhen { get; set; }



        public static IQueryOperations<AnalysisResult> Operations
        {
            get;
            private set;
        }

        static AnalysisResult()
        {
            Operations = new QueryOperations<AnalysisResult>(
                //Smallerthan
                          (q, ar) => q.Where(row => row.AnalysisID < ar.AnalysisID || row.Result.CompareTo(ar.Result) < 0),
                //Equals
                          (q, ar) => q.Where(row => row.AnalysisID == ar.AnalysisID && row.Result == ar.Result),
                //Orderby
                          (q) => from ar in q
                                 orderby ar.AnalysisID, ar.Result
                                 select ar,
                //FreeKey
                          (q, ar) =>
                          {
                              //Nothing To Do. 
                          });
        }
    }
}
