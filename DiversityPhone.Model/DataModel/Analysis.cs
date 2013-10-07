using Microsoft.Phone.Data.Linq.Mapping;
using System;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;


namespace DiversityPhone.Model
{
    [Table]
#if !TEST
    [Index(Columns = "LastUsed", IsUnique = false, Name = "term_lastusage")]
#endif
    public class Analysis
    {
        //Read-Only

        public static readonly DateTime DefaultLastUsed = new DateTime(2009, 01, 01); // DateTime.MinValue creates an overflow on insert.

        public Analysis()
        {
            this.LastUsed = DefaultLastUsed;
        }

        [Column(IsPrimaryKey = true)]
        public int AnalysisID { get; set; }

        [Column]
        public string DisplayText { get; set; }

        [Column]
        public string Description { get; set; }

        [Column]
        public string MeasurementUnit { get; set; }


        [Column]
        public DateTime LastUsed { get; set; }

        public string TextAndUnit
        {
            get
            {
                StringBuilder sb = new StringBuilder(DisplayText);
                if (!string.IsNullOrWhiteSpace(MeasurementUnit))
                {
                    sb.Append(" in ").Append(MeasurementUnit);
                }
                return sb.ToString();
            }
        }


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
