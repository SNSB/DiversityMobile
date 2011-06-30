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
using SterlingDemo.Model;

namespace WindowsPhoneSterling.Sterling
{
    public class ISODatabase : BaseDatabaseInstance 
    {
        public override string Name
        {
            get { return "ISODatabase"; }
        }

        protected override System.Collections.Generic.List<ITableDefinition> RegisterTables()
        {
            return new System.Collections.Generic.List<ITableDefinition>
            {
                CreateTableDefinition<StringISO,Guid>(i=>i.GUID)
            };
        }
    }
}
