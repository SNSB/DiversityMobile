

using System;
using System.Linq;
using ReactiveUI;
using Microsoft.Phone.Data.Linq.Mapping;
using System.Data.Linq.Mapping;
using Svc = DiversityPhone.DiversityService;

namespace DiversityPhone.Model
{	
	[Table]
	public class IdentificationUnitAnalysis : ReactiveObject, IModifyable
	{
		
		private int _IdentificationUnitAnalysisID;
		[Column(IsPrimaryKey=true)]
		public int IdentificationUnitAnalysisID
		{
			get { return _IdentificationUnitAnalysisID; }
			set 
			{
				if (_IdentificationUnitAnalysisID != value)
				{
					this.raisePropertyChanging("IdentificationUnitAnalysisID");
					_IdentificationUnitAnalysisID = value;
					this.raisePropertyChanged("IdentificationUnitAnalysisID");
				}  
			}
		}
		  

		
		private int _IdentificationUnitID;
		[Column]
		public int IdentificationUnitID
		{
			get { return _IdentificationUnitID; }
			set 
			{
				if (_IdentificationUnitID != value)
				{
					this.raisePropertyChanging("IdentificationUnitID");
					_IdentificationUnitID = value;
					this.raisePropertyChanged("IdentificationUnitID");
				}  
			}
		}
		   
		
		private int _AnalysisID;
		[Column]
		public int AnalysisID
		{
			get { return _AnalysisID; }
			set 
			{
				if (_AnalysisID != value)
				{
					this.raisePropertyChanging("AnalysisID");
					_AnalysisID = value;
					this.raisePropertyChanged("AnalysisID");
				}  
			}
		}
		 

		
		private string _DisplayText;
		[Column]
		public string DisplayText
		{
			get { return _DisplayText; }
			set 
			{
				if (_DisplayText != value)
				{
					this.raisePropertyChanging("DisplayText");
					_DisplayText = value;
					this.raisePropertyChanged("DisplayText");
				}  
			}
		}
				
		private string _AnalysisResult;
		[Column]
		public string AnalysisResult
		{
			get { return _AnalysisResult; }
			set 
			{
				if (_AnalysisResult != value)
				{
					this.raisePropertyChanging("AnalysisResult");
					_AnalysisResult = value;
					this.raisePropertyChanged("AnalysisResult");
				}  
			}
		}
		
		
		private DateTime _AnalysisDate;
		[Column]
		public DateTime AnalysisDate
		{
			get { return _AnalysisDate; }
			set 
			{
				if (_AnalysisDate != value)
				{
					this.raisePropertyChanging("AnalysisDate");
					_AnalysisDate = value;
					this.raisePropertyChanged("AnalysisDate");
				}  
			}
		}
		  
		
		private ModificationState _ModificationState;
		[Column]
		public ModificationState ModificationState
		{
			get { return _ModificationState; }
			set 
			{
				if (_ModificationState != value)
				{
					this.raisePropertyChanging("ModificationState");
					_ModificationState = value;
					this.raisePropertyChanged("ModificationState");
				}  
			}
		}
		
		
		private int? _DiversityCollectionUnitID;
		[Column(CanBeNull=true)]
		public int? DiversityCollectionUnitID
		{
			get { return _DiversityCollectionUnitID; }
			set 
			{
				if (_DiversityCollectionUnitID != value)
				{
					this.raisePropertyChanging("DiversityCollectionUnitID");
					_DiversityCollectionUnitID = value;
					this.raisePropertyChanged("DiversityCollectionUnitID");
				}  
			}
		}
		 

        public IdentificationUnitAnalysis()
        {
            this.ModificationState = ModificationState.New;            
            this.AnalysisDate = DateTime.Now;
            this.DiversityCollectionUnitID = null;            
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
 
