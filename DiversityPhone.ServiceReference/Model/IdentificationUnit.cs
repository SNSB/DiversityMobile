

using System;
using System.Linq;
using ReactiveUI;
using Microsoft.Phone.Data.Linq.Mapping;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Svc = DiversityPhone.DiversityService;

namespace DiversityPhone.Model
{	
	[Table]
	[Index(Columns="RelatedUnitID", IsUnique=false, Name="relunit_idx")] 
	public class IdentificationUnit : ReactiveObject, ILocalizable, IModifyable, IMultimediaOwner
	{  
#pragma warning disable 0169
		[Column(IsVersion = true)]
		private Binary version;
#pragma warning restore 0169

		
		private int _UnitID;
		[Column(IsPrimaryKey=true)]
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
		  
		
		private int? _CollectionUnitID;
		[Column(CanBeNull=true)]
		public int? CollectionUnitID
		{
			get { return _CollectionUnitID; }
			set 
			{
				if (_CollectionUnitID != value)
				{
					this.raisePropertyChanging("CollectionUnitID");
					_CollectionUnitID = value;
					this.raisePropertyChanged("CollectionUnitID");
				}  
			}
		}
		    
		
		private int _SpecimenID;
		[Column]
		public int SpecimenID
		{
			get { return _SpecimenID; }
			set 
			{
				if (_SpecimenID != value)
				{
					this.raisePropertyChanging("SpecimenID");
					_SpecimenID = value;
					this.raisePropertyChanged("SpecimenID");
				}  
			}
		}
		 
		
		private int? _RelatedUnitID;
		[Column(CanBeNull=true)]
		public int? RelatedUnitID
		{
			get { return _RelatedUnitID; }
			set 
			{
				if (_RelatedUnitID != value)
				{
					this.raisePropertyChanging("RelatedUnitID");
					_RelatedUnitID = value;
					this.raisePropertyChanged("RelatedUnitID");
				}  
			}
		}
		 
		
		private bool _OnlyObserved;
		[Column]
		public bool OnlyObserved
		{
			get { return _OnlyObserved; }
			set 
			{
				if (_OnlyObserved != value)
				{
					this.raisePropertyChanging("OnlyObserved");
					_OnlyObserved = value;
					this.raisePropertyChanged("OnlyObserved");
				}  
			}
		}
		    
		
		private string _TaxonomicGroup;
		[Column]
		public string TaxonomicGroup
		{
			get { return _TaxonomicGroup; }
			set 
			{
				if (_TaxonomicGroup != value)
				{
					this.raisePropertyChanging("TaxonomicGroup");
					_TaxonomicGroup = value;
					this.raisePropertyChanged("TaxonomicGroup");
				}  
			}
		}
				
		private string _RelationType;
		[Column]
		public string RelationType
		{
			get { return _RelationType; }
			set 
			{
				if (_RelationType != value)
				{
					this.raisePropertyChanging("RelationType");
					_RelationType = value;
					this.raisePropertyChanged("RelationType");
				}  
			}
		}
				
		private string _Qualification;
		[Column]
		public string Qualification
		{
			get { return _Qualification; }
			set 
			{
				if (_Qualification != value)
				{
					this.raisePropertyChanging("Qualification");
					_Qualification = value;
					this.raisePropertyChanged("Qualification");
				}  
			}
		}
				
		private string _WorkingName;
		[Column]
		public string WorkingName
		{
			get { return _WorkingName; }
			set 
			{
				if (_WorkingName != value)
				{
					this.raisePropertyChanging("WorkingName");
					_WorkingName = value;
					this.raisePropertyChanged("WorkingName");
				}  
			}
		}
				
		private string _IdentificationUri;
		[Column]
		public string IdentificationUri
		{
			get { return _IdentificationUri; }
			set 
			{
				if (_IdentificationUri != value)
				{
					this.raisePropertyChanging("IdentificationUri");
					_IdentificationUri = value;
					this.raisePropertyChanged("IdentificationUri");
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
		
		
		private double? _Altitude;
		[Column(CanBeNull=true,UpdateCheck=UpdateCheck.Never)]
		public double? Altitude
		{
			get { return _Altitude; }
			set 
			{
				if (_Altitude != value)
				{
					this.raisePropertyChanging("Altitude");
					_Altitude = value;
					this.raisePropertyChanged("Altitude");
				}  
			}
		}
		    
		
		private double? _Latitude;
		[Column(CanBeNull=true,UpdateCheck=UpdateCheck.Never)]
		public double? Latitude
		{
			get { return _Latitude; }
			set 
			{
				if (_Latitude != value)
				{
					this.raisePropertyChanging("Latitude");
					_Latitude = value;
					this.raisePropertyChanged("Latitude");
				}  
			}
		}
		    
		
		private double? _Longitude;
		[Column(CanBeNull=true,UpdateCheck=UpdateCheck.Never)]
		public double? Longitude
		{
			get { return _Longitude; }
			set 
			{
				if (_Longitude != value)
				{
					this.raisePropertyChanging("Longitude");
					_Longitude = value;
					this.raisePropertyChanged("Longitude");
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
		
		  
		public IdentificationUnit()
        {
            this.ModificationState = ModificationState.New;            

            this.AnalysisDate = DateTime.Now;//TODO Something useful?
            this.RelatedUnitID = null;
        }

        public static IQueryOperations<IdentificationUnit> Operations
        {
            get;
            private set;
        }

        static IdentificationUnit()
        {
            Operations = new QueryOperations<IdentificationUnit>(
                //Smallerthan
                          (q, iu) => q.Where(row => row.UnitID < iu.UnitID),
                //Equals
                          (q, iu) => q.Where(row => row.UnitID == iu.UnitID),
                //Orderby
                          (q) => q.OrderBy(iu => iu.UnitID),
                //FreeKey
                          (q, iu) =>
                          {
                              iu.UnitID = QueryOperations<IdentificationUnit>.FindFreeIntKey(q, row => row.UnitID);
                          });
        }

        public DBObjectType OwnerType
        {
            get { return DBObjectType.IdentificationUnit; }
        }

        public int OwnerID
        {
            get { return UnitID; }
        }
    }	
}
 
