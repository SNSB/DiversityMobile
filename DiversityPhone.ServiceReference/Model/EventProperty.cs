

using System;
using System.Linq;
using ReactiveUI;
using Microsoft.Phone.Data.Linq.Mapping;
using System.Data.Linq.Mapping;
using Svc = DiversityPhone.DiversityService;

namespace DiversityPhone.Model
{	
	[Table]
	public class EventProperty : ReactiveObject, IModifyable
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
		   
		
		private int _PropertyID;
		[Column(IsPrimaryKey=true)]
		public int PropertyID
		{
			get { return _PropertyID; }
			set 
			{
				if (_PropertyID != value)
				{
					this.raisePropertyChanging("PropertyID");
					_PropertyID = value;
					this.raisePropertyChanged("PropertyID");
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
				
		private string _PropertyUri;
		[Column]
		public string PropertyUri
		{
			get { return _PropertyUri; }
			set 
			{
				if (_PropertyUri != value)
				{
					this.raisePropertyChanging("PropertyUri");
					_PropertyUri = value;
					this.raisePropertyChanged("PropertyUri");
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
		   

  
		 public EventProperty()
        {            
            this.ModificationState = ModificationState.New;
        }


        public static IQueryOperations<EventProperty> Operations
        {
            get;
            private set;
        }

        static EventProperty()
        {
            Operations = new QueryOperations<EventProperty>(
                //Smallerthan
                          (q, cep) => q.Where(row => row.EventID < cep.EventID || row.PropertyID < cep.PropertyID),
                //Equals
                          (q, cep) => q.Where(row => row.EventID == cep.EventID && row.PropertyID == cep.PropertyID),
                //Orderby
                          (q) => from cep in q
                                 orderby cep.EventID, cep.PropertyID
                                 select cep,
                //FreeKey
                          (q, cep) =>
                          {
                              //Not Applicable
                          });
        }

        public static Svc.CollectionEventProperty ConvertToServiceObject(EventProperty cep)
        {
            Svc.CollectionEventProperty export = new Svc.CollectionEventProperty();
            export.DisplayText = cep.DisplayText;
            export.EventID = cep.EventID;

            export.PropertyID = cep.PropertyID;
            export.PropertyUri = cep.PropertyUri;
            export.DiversityCollectionEventID = cep.DiversityCollectionEventID;
            return export;
        }
    }
}
 
