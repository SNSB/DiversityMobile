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
using Wintellect.Sterling;

namespace DiversityPhone.Services
{
    public class OfflineStorage : IOfflineStorage
    {
        private ISterlingDatabaseInstance _db { get; set; }
        public OfflineStorage(ISterlingDatabaseInstance db)
        {
            _db = db;
        }


        public System.Collections.Generic.IList<global::DiversityService.Model.EventSeries> EventSeries
        {
            get { throw new NotImplementedException(); }
        }
    }
}
