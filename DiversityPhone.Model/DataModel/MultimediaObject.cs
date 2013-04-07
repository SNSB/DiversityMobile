

using ReactiveUI;
using System;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace DiversityPhone.Model
{
    [Table]
    public class MultimediaObject : ReactiveObject, IModifyable, IEquatable<MultimediaObject>
    {
#pragma warning disable 0169
		[Column(IsVersion = true)]
		private Binary version;
#pragma warning restore 0169

		
		private int _MMOID;
		[Column(IsPrimaryKey=true,IsDbGenerated=true)]
		[EntityKey]
		public int MMOID
		{
			get { return _MMOID; }
			set 
			{
				if (_MMOID != value)
				{
					this.raisePropertyChanging("MMOID");
					_MMOID = value;
					this.raisePropertyChanged("MMOID");
				}  
			}
		}
		   
		
		private DBObjectType _OwnerType;
		[Column]
		
		public DBObjectType OwnerType
		{
			get { return _OwnerType; }
			set 
			{
				if (_OwnerType != value)
				{
					this.raisePropertyChanging("OwnerType");
					_OwnerType = value;
					this.raisePropertyChanged("OwnerType");
				}  
			}
		}
		   
		
		private int _RelatedId;
		[Column]
		
		public int RelatedId
		{
			get { return _RelatedId; }
			set 
			{
				if (_RelatedId != value)
				{
					this.raisePropertyChanging("RelatedId");
					_RelatedId = value;
					this.raisePropertyChanged("RelatedId");
				}  
			}
		}
		    
		
		private string _Uri;
		[Column]
		
		public string Uri
		{
			get { return _Uri; }
			set 
			{
				if (_Uri != value)
				{
					this.raisePropertyChanging("Uri");
					_Uri = value;
					this.raisePropertyChanged("Uri");
				}  
			}
		}
		
		
		private MediaType _MediaType;
		[Column]
		
		public MediaType MediaType
		{
			get { return _MediaType; }
			set 
			{
				if (_MediaType != value)
				{
					this.raisePropertyChanging("MediaType");
					_MediaType = value;
					this.raisePropertyChanged("MediaType");
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
		
		
		private string _CollectionUri;
		[Column]
		
		public string CollectionUri
		{
			get { return _CollectionUri; }
			set 
			{
				if (_CollectionUri != value)
				{
					this.raisePropertyChanging("CollectionUri");
					_CollectionUri = value;
					this.raisePropertyChanged("CollectionUri");
				}  
			}
		}
		   

        

        public static IQueryOperations<MultimediaObject> Operations
        {
            get;
            private set;
        }

        public override int GetHashCode()
        {
            return MMOID.GetHashCode() ^
                OwnerType.GetHashCode() ^
                RelatedId.GetHashCode() ^
                (Uri ?? "").GetHashCode() ^
                MediaType.GetHashCode();
        }

        static MultimediaObject()
        {
            Operations = new QueryOperations<MultimediaObject>(
                //Smallerthan
                         (q, mmo) => q.Where(row => row.MMOID < mmo.MMOID),
                //Equals
                         (q, mmo) => q.Where(row => row.MMOID == mmo.MMOID),
                //Orderby
                         (q) => from mmo in q
                                orderby mmo.MediaType, mmo.OwnerType, mmo.RelatedId
                                select mmo,
                //FreeKey
                         (q, mmo) =>
                         {
                             mmo.MMOID = QueryOperations<MultimediaObject>.FindFreeIntKey(q, row => row.MMOID);
                         });
        }

        public bool Equals(MultimediaObject other)
        {
            return base.Equals(other) ||
               (this.MediaType == other.MediaType &&
                this.MMOID == other.MMOID &&
                this.OwnerType == other.OwnerType &&
                this.RelatedId == other.RelatedId &&
                this.Uri == other.Uri &&                
                this.ModificationState == other.ModificationState);
        }
    }
}
 
