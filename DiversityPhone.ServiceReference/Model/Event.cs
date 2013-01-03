

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
	public class Event : ReactiveObject, ILocalizable, IModifyable, IMultimediaOwner
	{
		
		private int _EventID;
		[Column(IsPrimaryKey=true)]
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
		   
		
		private int? _SeriesID;
		[Column(CanBeNull=true)]
		public int? SeriesID
		{
			get { return _SeriesID; }
			set 
			{
				if (_SeriesID != value)
				{
					this.raisePropertyChanging("SeriesID");
					_SeriesID = value;
					this.raisePropertyChanged("SeriesID");
				}  
			}
		}
		   
		
		private DateTime _CollectionDate;
		[Column(UpdateCheck=UpdateCheck.Never)]
		public DateTime CollectionDate
		{
			get { return _CollectionDate; }
			set 
			{
				if (_CollectionDate != value)
				{
					this.raisePropertyChanging("CollectionDate");
					_CollectionDate = value;
					this.raisePropertyChanged("CollectionDate");
				}  
			}
		}
		    
		
		private string _LocalityDescription;
		[Column]
		public string LocalityDescription
		{
			get { return _LocalityDescription; }
			set 
			{
				if (_LocalityDescription != value)
				{
					this.raisePropertyChanging("LocalityDescription");
					_LocalityDescription = value;
					this.raisePropertyChanged("LocalityDescription");
				}  
			}
		}
				
		private string _HabitatDescription;
		[Column]
		public string HabitatDescription
		{
			get { return _HabitatDescription; }
			set 
			{
				if (_HabitatDescription != value)
				{
					this.raisePropertyChanging("HabitatDescription");
					_HabitatDescription = value;
					this.raisePropertyChanged("HabitatDescription");
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
		    
		
		private DateTime? _DeterminationDate;
		[Column(CanBeNull=true,UpdateCheck=UpdateCheck.Never)]
		public DateTime? DeterminationDate
		{
			get { return _DeterminationDate; }
			set 
			{
				if (_DeterminationDate != value)
				{
					this.raisePropertyChanging("DeterminationDate");
					_DeterminationDate = value;
					this.raisePropertyChanged("DeterminationDate");
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
		   
		
		private int? _DiversityCollectionSeriesID;
		[Column(CanBeNull=true)]
		public int? DiversityCollectionSeriesID
		{
			get { return _DiversityCollectionSeriesID; }
			set 
			{
				if (_DiversityCollectionSeriesID != value)
				{
					this.raisePropertyChanging("DiversityCollectionSeriesID");
					_DiversityCollectionSeriesID = value;
					this.raisePropertyChanged("DiversityCollectionSeriesID");
				}  
			}
		}
		 
  
		public Event()
        {
            this.SeriesID = null;
            this.CollectionDate = DateTime.Now;            
            this.ModificationState = ModificationState.New;
		}

       
        public static IQueryOperations<Event> Operations
        {
            get;
            private set;
        }

        static Event()
        {
            Operations = new QueryOperations<Event>(
                //Smallerthan
                          (q, ev) => q.Where(row => row.EventID < ev.EventID),
                //Equals
                          (q, ev) => q.Where(row => row.EventID == ev.EventID),
                //Orderby
                          (q) => q.OrderBy(ev => ev.EventID),
                //FreeKey
                          (q, ev) =>
                          {
                              ev.EventID = QueryOperations<Event>.FindFreeIntKey(q, row => row.EventID);
                          });
        }       

        public ReferrerType OwnerType
        {
            get { return ReferrerType.Event; }
        }


        public int OwnerID
        {
            get { return EventID; }
        }


		public static Svc.Event ConvertToServiceObject(Event ev)
        {
            Svc.Event export = new Svc.Event();
            if (ev.DiversityCollectionEventID.HasValue)
                export.DiversityCollectionEventID = ev.DiversityCollectionEventID.Value;
            else
                export.DiversityCollectionEventID = Int32.MinValue;
            export.DiversityCollectionSeriesID = ev.DiversityCollectionSeriesID;
            export.Altitude = ev.Altitude;
            export.CollectionDate = ev.CollectionDate;
            export.DeterminationDate = ev.DeterminationDate;
            export.EventID = ev.EventID;
            export.HabitatDescription = ev.HabitatDescription;
            export.Latitude = ev.Latitude;
            export.LocalityDescription = ev.LocalityDescription;            
            export.Longitude = ev.Longitude;
            export.SeriesID = ev.SeriesID;
            return export;
        }
    }	
}
 
