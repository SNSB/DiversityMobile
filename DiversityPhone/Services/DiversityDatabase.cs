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
using DiversityPhone.DiversityService;

namespace DiversityPhone.Services
{
    public class DiversityDatabase : BaseDatabaseInstance, IDiversityDatabase
    {
        public const string LOCATION_DESCRIPTION_UPPER = "LocationDescription";
        public const string SPECIMEN_EVENTID = "SpecEventID";
        public const string IDENTIFICATIONUNIT_RELATED = "RelatedIU";
        public override string Name
        {
            get
            {
                return "DiversityDatabase";
            }
        }

        protected override List<ITableDefinition> RegisterTables()
        {
            var esTable = CreateTableDefinition<CollectionEventSeries, int>(x => x.SeriesID);
            var evTable = CreateTableDefinition<CollectionEvent, int>(ce => ce.CollectionEventID).WithIndex<CollectionEvent, string, int>(LOCATION_DESCRIPTION_UPPER, ce => (ce.LocalityDescription != null) ? ce.LocalityDescription.ToUpper() : "No Description".ToUpper());
            var iuTable = CreateTableDefinition<IdentificationUnit, int>(iu => iu.IdentificationUnitID).WithIndex<IdentificationUnit, int, int>(IDENTIFICATIONUNIT_RELATED, iu => iu.RelatedUnitID ?? int.MinValue);
            


            return new List<ITableDefinition>
            {
                esTable,
                evTable,
                iuTable
                

            };
        }

        #region IDiversityDatabase

        #endregion
    }
}
