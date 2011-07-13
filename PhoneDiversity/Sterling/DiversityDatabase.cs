using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Wintellect.Sterling.Database;
using System.Collections.Generic;
using SterlingToLINQ.DiversityService;

namespace SterlingToLINQ.Sterling
{
    public class DiversityDatabase : BaseDatabaseInstance
    {
        public const string LOCATION_DESCRIPTION_UPPER = "LocationDescription";
        public const string SPECIMEN_EVENTID = "SpecEventID";
        public override string Name
        {
            get
            {
                return "DiversityDatabase";
            }
        }

        protected override List<ITableDefinition> RegisterTables()
        {
            var ceTable = CreateTableDefinition<CollectionEvent, int>(ce => ce.CollectionEventID).WithIndex<CollectionEvent, string, int>(LOCATION_DESCRIPTION_UPPER, ce => (ce.LocalityDescription != null) ? ce.LocalityDescription.ToUpper() : "No Description".ToUpper());
            

            return new List<ITableDefinition>
            {
                ceTable,
                CreateTableDefinition<CollectionSpecimen,int>(x=>x.CollectionSpecimenID).WithIndex<CollectionSpecimen,int?,int>(SPECIMEN_EVENTID,s=>s.CollectionEventID)

            };
        }
    }
}
