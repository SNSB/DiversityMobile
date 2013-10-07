

using ReactiveUI;
using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;

namespace DiversityPhone.Model
{	
	[Table]
	public class Event : ReactiveObject, ILocalizable, IModifyable, IMultimediaOwner
	{
#pragma warning disable 0169
		[Column(IsVersion = true)]
		private Binary version;
#pragma warning restore 0169

		
		private int _EventID;
		[Column(IsPrimaryKey=true,IsDbGenerated=true)]
		[EntityKey]
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
		   
		
		private int? _CollectionEventID;
		[Column(CanBeNull=true)]
		
		public int? CollectionEventID
		{
			get { return _CollectionEventID; }
			set 
			{
				

				if (_CollectionEventID != value)
				{
					this.raisePropertyChanging("CollectionEventID");
					_CollectionEventID = value;
					this.raisePropertyChanged("CollectionEventID");
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
				
				var minSQLCEDate = new DateTime(1753, 01, 01);
				var maxSQLCEDate = new DateTime(9999, 12, 31);
				if (value < minSQLCEDate)
					value = minSQLCEDate;
				if (value > maxSQLCEDate)
					value = maxSQLCEDate;
				

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
		
		public Event()
        {
            this.SeriesID = null;
            this.CollectionDate = DateTime.Now;            
            this.ModificationState = ModificationState.New;
		}

 		public new Event MemberwiseClone()
        {
            return (Event)base.MemberwiseClone();
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

        public DBObjectType EntityType
        {
            get { return DBObjectType.Event; }
        }


        public int EntityID
        {
            get { return EventID; }
        }
    }	
}
 
