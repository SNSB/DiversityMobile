


using System;
using System.Linq;
using ReactiveUI;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace DiversityPhone.Model
{	
	[Table]
	public class Specimen : ReactiveObject, IModifyable, IMultimediaOwner
	{
#pragma warning disable 0169
		[Column(IsVersion = true)]
		private Binary version;
#pragma warning restore 0169

		
		private int _SpecimenID;
		[Column(IsPrimaryKey=true,IsDbGenerated=true)]
		[EntityKey]
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
				
		private int? _CollectionSpecimenID;
		[Column(CanBeNull=true)]
		
		public int? CollectionSpecimenID
		{
			get { return _CollectionSpecimenID; }
			set 
			{
				if (_CollectionSpecimenID != value)
				{
					this.raisePropertyChanging("CollectionSpecimenID");
					_CollectionSpecimenID = value;
					this.raisePropertyChanged("CollectionSpecimenID");
				}  
			}
		}
		   
		
		private int _EventID;
		[Column]
		
		public int EventID
		{
			get { return _EventID; }
			set 
			{
				if (_EventID != value)
				{
					this.raisePropertyChanging("EventID");
					_EventID = value;
					this.raisePropertyChanged("EventID");
				}  
			}
		}
		
		
		private string _AccessionNumber;
		[Column]
		
		public string AccessionNumber
		{
			get { return _AccessionNumber; }
			set 
			{
				if (_AccessionNumber != value)
				{
					this.raisePropertyChanging("AccessionNumber");
					_AccessionNumber = value;
					this.raisePropertyChanged("AccessionNumber");
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
		
        public Specimen()
        {
            this.AccessionNumber = null;            
            this.ModificationState = ModificationState.New;            
        }


        public static IQueryOperations<Specimen> Operations
        {
            get;
            private set;
        }

        static Specimen()
        {
            Operations = new QueryOperations<Specimen>(
                //Smallerthan
                          (q, spec) => q.Where(row => row.SpecimenID < spec.SpecimenID),
                //Equals
                          (q, spec) => q.Where(row => row.SpecimenID == spec.SpecimenID),
                //Orderby
                          (q) => q.OrderBy(spec => spec.SpecimenID),
                //FreeKey
                          (q, spec) =>
                          {
                              spec.SpecimenID = QueryOperations<Specimen>.FindFreeIntKey(q, row => row.SpecimenID);
                          });
        }

        public DBObjectType EntityType
        {
            get { return DBObjectType.Specimen; }
        }

        public int EntityID
        {
            get { return SpecimenID; }
        }
    }


    public static class SpecimenMixin
    {
        public static bool IsObservation(this Specimen spec)
        {
            return spec.AccessionNumber == null
                && !spec.IsNew();
        }

        public static Specimen MakeObservation(this Specimen spec)
        {
            spec.AccessionNumber = null;
            return spec;
        }
    }
} 