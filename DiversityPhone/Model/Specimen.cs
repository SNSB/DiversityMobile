﻿namespace DiversityPhone.Model
{
    using System;
    using System.Linq;
    using System.Data.Linq.Mapping;
    using DiversityPhone.Services;
    using Svc = DiversityPhone.DiversityService;
    using System.Data.Linq;
using ReactiveUI;

    [Table]
    public class Specimen : ReactiveObject, IModifyable, IMultimediaOwner
    {
        [Column(IsPrimaryKey = true)]
        public int SpecimenID { get; set; }

        [Column(CanBeNull = true)]
        public int? DiversityCollectionSpecimenID { get; set; }

        [Column]
        public int EventID { get; set; }

        [Column(CanBeNull = true)]
        public int? DiversityCollectionEventID { get; set; }

        private string _AccessionNumber;
        [Column]
        public string AccessionNumber
        {
            get { return _AccessionNumber; }
            set { this.RaiseAndSetIfChanged(x => x.AccessionNumber, ref _AccessionNumber, value); }
        }


        ModificationState _ModificationState;
        [Column]
        public ModificationState ModificationState
        {
            get { return _ModificationState; }
            set { this.RaiseAndSetIfChanged(x => x.ModificationState, ref _ModificationState, value); }
        }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }


        public Specimen()
        {
            this.AccessionNumber = null;
            this.LogUpdatedWhen = DateTime.Now;
            this.ModificationState = ModificationState.New;
            this.DiversityCollectionSpecimenID = null;
            //_Units = new EntitySet<IdentificationUnit>(
            //  new Action<IdentificationUnit>(Attach_Unit),
            //  new Action<IdentificationUnit>(Detach_Unit));
            //_Event = default(EntityRef<Event>);
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

        #region Associations
        //private EntitySet<IdentificationUnit> _Units;
        //[Association(Name = "FK_Specimen_Units",
        //             Storage = "_Units",
        //             ThisKey = "CollectionSpecimenID",
        //             OtherKey = "SpecimenID",
        //             IsForeignKey = true,
        //             DeleteRule = "CASCADE")]
        //public EntitySet<IdentificationUnit> Units
        //{
        //    get { return _Units; }
        //    set { _Units.Assign(value); }
        //}
        
        //private EntityRef<Event> _Event;
        //[Association(Name = "FK_Specimen_Event",
        //        Storage = "_Event",
        //        ThisKey = "CollectionEventID",
        //        OtherKey = "EventID",
        //        IsForeignKey=true,
        //        DeleteOnNull=true)]
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
        //                previousValue.Specimen.Remove(this);
        //            }
        //            this._Event.Entity = value;
        //            if ((value != null))
        //            {
        //                value.Specimen.Add(this);
        //                this.CollectionEventID = value.EventID;
        //            }
        //            else
        //            {
        //                this.CollectionEventID = default(int);
        //            }
        //        }
        //    }
        //}

        //private void Attach_Unit(IdentificationUnit entity)
        //{
        //    entity.Specimen = this;
        //}

        //private void Detach_Unit(IdentificationUnit entity)
        //{
        //    entity.Specimen = null;
        //}

        #endregion
        //public static Specimen Clone(Specimen spec)
        //{
        //    throw new NotImplementedException();
        //}


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
