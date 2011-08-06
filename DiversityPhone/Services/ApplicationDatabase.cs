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

namespace DiversityPhone.Services
{
    public class ApplicationDatabase : BaseDatabaseInstance
    {
        public override string Name
        {
            get
            {
                return "DiversityPhoneApplicationData";
            }
        }

        protected override System.Collections.Generic.List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
            {

            };
        }
    }
}
