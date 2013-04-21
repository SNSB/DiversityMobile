

using System;
using System.Linq;
using ReactiveUI;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace DiversityPhone.Model
{	
	[Table]
	public class IdentificationUnitAnalysis : ReactiveObject, IModifyable
	{
#pragma warning disable 0169
		[Column(IsVersion = true)]
		private Binary version;
#pragma warning restore 0169

		
		private int _IdentificationUnitAnalysisID;
		[Column(IsPrimaryKey=true,IsDbGenerated=true)]
		[EntityKey]
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
		  

		
		private int _UnitID;
		[Column]
		
		public int UnitID
		{
			get { return _UnitID; }
			set 
			{
				

				if (_UnitID != value)
				{
					this.raisePropertyChanging("UnitID");
					_UnitID = value;
					this.raisePropertyChanged("UnitID");
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
				
				var minSQLCEDate = new DateTime(1753, 01, 01);
				var maxSQLCEDate = new DateTime(9999, 12, 31);
				if (value < minSQLCEDate)
					value = minSQLCEDate;
				if (value > maxSQLCEDate)
					value = maxSQLCEDate;
				

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
		
        public IdentificationUnitAnalysis()
        {
            this.ModificationState = ModificationState.New;            
            this.AnalysisDate = DateTime.Now;                      
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
    }
}
 
