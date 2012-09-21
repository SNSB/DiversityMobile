

using System;
using System.Linq;
using ReactiveUI;
using Microsoft.Phone.Data.Linq.Mapping;
using System.Data.Linq.Mapping;
using Svc = DiversityPhone.DiversityService;

namespace DiversityPhone.Model
{	
	[Table]
	public class Localization : ReactiveObject, IModifyable, ILocalizable
	{
		
		private int _LocalizationID;
		[Column(IsPrimaryKey=true)]
		public int LocalizationID
		{
			get { return _LocalizationID; }
			set 
			{
				if (_LocalizationID != value)
				{
					this.raisePropertyChanging("LocalizationID");
					_LocalizationID = value;
					this.raisePropertyChanged("LocalizationID");
				}  
			}
		}
		   

		
		private int _RelatedID;
		[Column]
		public int RelatedID
		{
			get { return _RelatedID; }
			set 
			{
				if (_RelatedID != value)
				{
					this.raisePropertyChanging("RelatedID");
					_RelatedID = value;
					this.raisePropertyChanged("RelatedID");
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
			}
}
 
