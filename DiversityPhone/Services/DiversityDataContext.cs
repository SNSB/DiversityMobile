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
using System.Data.Linq;
using DiversityService.Model;
using System.Data.Linq.Mapping;
using System.IO;


namespace DiversityPhone.Services
{
    public class DiversityDataContext : DataContext
    {
        static XmlMappingSource source = XmlMappingSource.FromXml(mappingXML());
        static string connStr = "isostore:/diversityDB.sdf";
        static string mappingXML()
        {
            var ResourceStream = Application.GetResourceStream(new Uri("/DiversityPhone;component/Data/mapping.xml", UriKind.Relative));

            if (ResourceStream != null)
            {
                Stream myFileStream = ResourceStream.Stream;
                if (myFileStream.CanRead)
                {
                    StreamReader myStreamReader = new StreamReader(myFileStream);
                 
                    return myStreamReader.ReadToEnd();
                }
            }
            return string.Empty;
        }

        public DiversityDataContext()
            :base(connStr,source)
        {

        }       

        public Table<EventSeries> EventSeries;
        //public Table<Event> Events;
        //public Table<IdentificationUnit> IdentificationUnits;
    }
}
