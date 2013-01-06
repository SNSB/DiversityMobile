﻿


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
	public class EventSeries : ReactiveObject, IModifyable, ILocationOwner
	{
#pragma warning disable 0169
		[Column(IsVersion = true)]
		private Binary version;
#pragma warning restore 0169

		
		private int? _SeriesID;
		[Column(IsPrimaryKey=true)]
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
				
		private int? _CollectionSeriesID;
		[Column(CanBeNull=true)]
		public int? CollectionSeriesID
		{
			get { return _CollectionSeriesID; }
			set 
			{
				if (_CollectionSeriesID != value)
				{
					this.raisePropertyChanging("CollectionSeriesID");
					_CollectionSeriesID = value;
					this.raisePropertyChanged("CollectionSeriesID");
				}  
			}
		}
		   

		
		private DateTime _SeriesStart;
		[Column]
		public DateTime SeriesStart
		{
			get { return _SeriesStart; }
			set 
			{
				if (_SeriesStart != value)
				{
					this.raisePropertyChanging("SeriesStart");
					_SeriesStart = value;
					this.raisePropertyChanged("SeriesStart");
				}  
			}
		}
		  
		
		private DateTime? _SeriesEnd;
		[Column(CanBeNull=true,UpdateCheck=UpdateCheck.Never)]
		public DateTime? SeriesEnd
		{
			get { return _SeriesEnd; }
			set 
			{
				if (_SeriesEnd != value)
				{
					this.raisePropertyChanging("SeriesEnd");
					_SeriesEnd = value;
					this.raisePropertyChanged("SeriesEnd");
				}  
			}
		}
		     
		
		private string _Description;
		[Column]
		public string Description
		{
			get { return _Description; }
			set 
			{
				if (_Description != value)
				{
					this.raisePropertyChanging("Description");
					_Description = value;
					this.raisePropertyChanged("Description");
				}  
			}
		}
				
		private string _SeriesCode;
		[Column]
		public string SeriesCode
		{
			get { return _SeriesCode; }
			set 
			{
				if (_SeriesCode != value)
				{
					this.raisePropertyChanging("SeriesCode");
					_SeriesCode = value;
					this.raisePropertyChanged("SeriesCode");
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
		 
  
		private static EventSeries _NoEventSeries;

        public EventSeries()
        {
            this.Description = string.Empty;
            this.SeriesCode = string.Empty;
            this.SeriesStart = DateTime.Now;
            this.SeriesEnd = null;
            this.SeriesID = 0;
            this.ModificationState = ModificationState.New;            
        }


        public static EventSeries NoEventSeries
        {
            get
            {
                return _NoEventSeries;
            }
        }
       
        public static bool isNoEventSeries(EventSeries es)
        {
            return _NoEventSeries == es;
        }

        public static IQueryOperations<EventSeries> Operations
        {
            get;
            private set;
        }

        static EventSeries()
        {
            Operations = new QueryOperations<EventSeries>(
                //Smallerthan
                          (q, es) => q.Where(row => row.SeriesID < es.SeriesID),
                //Equals
                          (q, es) => q.Where(row => row.SeriesID == es.SeriesID),
                //Orderby
                          (q) => q.OrderBy(es => -es.SeriesID),//As EventSeries have negative Values Unlike the other entities in DiversityCollection we need to order by the nagtive value to get the same behaviour
                //FreeKey
                          (q, es) =>
                          {
                              es.SeriesID = QueryOperations<EventSeries>.FindFreeIntKeyUp(q, row => row.SeriesID.Value);//CollectionEventSeries Autoinc-key is lowered by one by default in DiversityCollection. As we need to avoid Synchronisationconflicts we need to count in the other direction.
                          });

            _NoEventSeries = new EventSeries()
            {
                Description = "Events",
                SeriesCode = "No EventSeries",
                ModificationState = ModificationState.Unmodified,
                SeriesEnd = DateTime.Now,     
				SeriesID = null,        
            };
        }      

		public DBObjectType OwnerType
        {
            get { return DBObjectType.EventSeries; }
        }

        public int OwnerID
        {
            get { return SeriesID.Value; }
        }
    }	
} 