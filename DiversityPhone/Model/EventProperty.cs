using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Data.Linq.Mapping;
using DiversityPhone.Services;
using Svc = DiversityPhone.DiversityService;
using System.Data.Linq;
using ReactiveUI;

namespace DiversityPhone.Model
{
    [Table]
    public class EventProperty : ReactiveObject, IModifyable
    {
        public EventProperty()
        {
            LogUpdatedWhen = DateTime.Now;
            this.ModificationState = ModificationState.New;
        }

        [Column(IsPrimaryKey = true)]
        public int EventID { get; set; }

        [Column(IsPrimaryKey = true)]
        public int PropertyID { get; set; }

        [Column(CanBeNull = true)]
        public int? DiversityCollectionEventID { get; set; }

        private string _DisplayText;
        [Column]
        public String DisplayText 
        {
            get { return _DisplayText; }
            set { this.RaiseAndSetIfChanged(x => x.DisplayText, ref _DisplayText, value); }
        }

        [Column]
        public String PropertyUri { get; set; }




        ModificationState _ModificationState;
        [Column]
        public ModificationState ModificationState
        {
            get { return _ModificationState; }
            set { this.RaiseAndSetIfChanged(x => x.ModificationState, ref _ModificationState, value); }
        }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }



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
            export.LogUpdatedWhen = cep.LogUpdatedWhen;
            export.PropertyID = cep.PropertyID;
            export.PropertyUri = cep.PropertyUri;
            export.DiversityCollectionEventID = cep.DiversityCollectionEventID;
            return export;
        }

        //#region Associations
        //private EntityRef<Event> _Event;
        //[Association(Name = "FK_EventProperty_Event",
        //        Storage = "_Event",
        //        ThisKey = "EventID",
        //        OtherKey = "EventID",
        //        IsForeignKey = true)]
        //public Event Event 
        //{
        //    get { return _Event.Entity; }
        //    set
        //    {
        //        Event previousValue = this._Event.Entity;
        //        if (((previousValue != value) ||
        //            (this._Event.HasLoadedOrAssignedValue
        //             == false)))
        //        {
        //            if ((previousValue != null))
        //            {
        //                this._Event.Entity = null;
        //                previousValue.Properties.Remove(this);
        //            }
        //            this._Event.Entity = value;
        //            if ((value != null))
        //            {
        //                value.Properties.Add(this);
        //                this.EventID = value.EventID;
        //            }
        //            else
        //            {
        //                this.EventID = default(int);
        //            }
        //        }
        //    }
        //}
        //#endregion

    }
}
