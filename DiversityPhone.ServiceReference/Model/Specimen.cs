


using System;
using System.Linq;
using ReactiveUI;
using Microsoft.Phone.Data.Linq.Mapping;
using System.Data.Linq.Mapping;
using Svc = DiversityPhone.DiversityService;

namespace DiversityPhone.Model
{	
	[Table]
	public class Specimen : ReactiveObject, IModifyable, IMultimediaOwner
	{
		
		private int _SpecimenID;
		[Column(IsPrimaryKey=true)]
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
		 
		
		private int? _DiversityCollectionSpecimenID;
		[Column(CanBeNull=true)]
		public int? DiversityCollectionSpecimenID
		{
			get { return _DiversityCollectionSpecimenID; }
			set 
			{
				if (_DiversityCollectionSpecimenID != value)
				{
					this.raisePropertyChanging("DiversityCollectionSpecimenID");
					_DiversityCollectionSpecimenID = value;
					this.raisePropertyChanged("DiversityCollectionSpecimenID");
				}  
			}
		}
		   
		
		private int? _DiversityCollectionEventID;
		[Column(CanBeNull=true)]
		public int? DiversityCollectionEventID
		{
			get { return _DiversityCollectionEventID; }
			set 
			{
				if (_DiversityCollectionEventID != value)
				{
					this.raisePropertyChanging("DiversityCollectionEventID");
					_DiversityCollectionEventID = value;
					this.raisePropertyChanged("DiversityCollectionEventID");
				}  
			}
		}
		   
       



        public Specimen()
        {
            this.AccessionNumber = null;            
            this.ModificationState = ModificationState.New;
            this.DiversityCollectionSpecimenID = null;
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

        public static Svc.Specimen ConvertToServiceObject(Specimen spec)
        {
            Svc.Specimen export = new Svc.Specimen();
            if (spec.DiversityCollectionSpecimenID != null)
                export.DiversityCollectionSpecimenID = (int)spec.DiversityCollectionSpecimenID;
            else export.DiversityCollectionSpecimenID = Int32.MinValue;
            export.DiversityCollectionEventID = spec.DiversityCollectionEventID;
            export.AccessionNumber = spec.AccessionNumber;
            export.CollectionEventID = spec.EventID;
            export.CollectionSpecimenID = spec.SpecimenID;
            return export;
        }

        public ReferrerType OwnerType
        {
            get { return ReferrerType.Specimen; }
        }

        public int OwnerID
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