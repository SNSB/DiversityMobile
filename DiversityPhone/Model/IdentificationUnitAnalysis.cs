using System;
using System.Linq;
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
using DiversityPhone.Services;
using Svc = DiversityPhone.DiversityService;
using System.Data.Linq;
using ReactiveUI;


namespace DiversityPhone.Model
{
    [Table]
    public class IdentificationUnitAnalysis : ReactiveObject, IModifyable
    {
        [Column]
        public int IdentificationUnitID { get; set; }

        [Column]
        public int AnalysisID { get; set; }

        [Column(IsPrimaryKey = true)]
        public int IdentificationUnitAnalysisID { get; set; }

        [Column(CanBeNull = true)]
        public int? DiversityCollectionUnitID { get; set; }

        [Column]
        public string AnalysisResult { get; set; }

        [Column]
        public DateTime AnalysisDate { get; set; } //Datum mit Uhrzeit

        [Column]
        public string DisplayText { get; set; }

        ModificationState _ModificationState;
        [Column]
        public ModificationState ModificationState
        {
            get { return _ModificationState; }
            set { this.RaiseAndSetIfChanged(x => x.ModificationState, ref _ModificationState, value); }
        }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }        

        public IdentificationUnitAnalysis()
        {
            this.ModificationState = ModificationState.New;
            this.LogUpdatedWhen = DateTime.Now;
            this.AnalysisDate = DateTime.Now;
            this.DiversityCollectionUnitID = null;
            //_Unit = default(EntityRef<IdentificationUnit>);
        }

        public static IQueryOperations<IdentificationUnitAnalysis> Operations
        {
            get;
            private set;
        }

        static IdentificationUnitAnalysis()
        {
            Operations = new QueryOperations<IdentificationUnitAnalysis>(
                //Smallerthan
                          (q, iuan) => q.Where(row => row.IdentificationUnitAnalysisID < iuan.IdentificationUnitAnalysisID),
                //Equals
                          (q, iuan) => q.Where(row => row.IdentificationUnitAnalysisID == iuan.IdentificationUnitAnalysisID),
                //Orderby
                          (q) => q.OrderBy(iuan => iuan.IdentificationUnitAnalysisID),
                //FreeKey
                          (q, iuan) =>
                          {
                              iuan.IdentificationUnitAnalysisID = QueryOperations<IdentificationUnitAnalysis>.FindFreeIntKey(q, row => row.IdentificationUnitAnalysisID);
                          });
        }

        public static Svc.IdentificationUnitAnalysis ConvertToServiceObject(IdentificationUnitAnalysis iua, IdentificationUnit iu)
        {
            Svc.IdentificationUnitAnalysis export = new Svc.IdentificationUnitAnalysis();
            if (iu.DiversityCollectionSpecimenID != null)
                export.DiversityCollectionSpecimenID = (int)iu.DiversityCollectionSpecimenID;
            else
                export.DiversityCollectionSpecimenID = Int32.MinValue;
            export.SpecimenID = iu.SpecimenID;
            export.DiversityCollectionUnitID = iua.DiversityCollectionUnitID;
            export.AnalysisDate = iua.AnalysisDate;
            export.AnalysisID = iua.AnalysisID;
            export.AnalysisResult = iua.AnalysisResult;
            export.IdentificationUnitAnalysisID = iua.IdentificationUnitAnalysisID;
            export.IdentificationUnitID = iua.IdentificationUnitID;
            return export;
        }

    }
}