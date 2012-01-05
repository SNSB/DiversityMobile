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
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml.Linq;

namespace DiversityPhone.Model
{
    //Class for savin Coordinates of Maps provided by the Map-Server in an XML-File
    public class ImageOptions
    {
        string name;
        string description;

        double nwlat;
        double nwlong;
        double selat;
        double selong;
        double swlat;
        double swlong;
        double nelat;
        double nelong;

        int? zoomlevel;
        int? transparency;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Description { get { return description; } set { description = value; } }

        public double NWLat { get { return nwlat; } set { nwlat = value; } }

        public double NWLong { get { return nwlong; } set { nwlong = value; } }

        public double SELat { get { return selat; } set { selat = value; } }

        public double SELong { get { return selong; } set { selong = value; } }

        public double SWLat { get { return swlat; } set { swlat = value; } }

        public double SWLong { get { return swlong; } set { swlong = value; } }

        public double NELat { get { return nelat; } set { nelat = value; } }
        
        public double NELong { get { return swlong; } set { swlong = value; } }

        public int? ZoomLevel { get { return zoomlevel; } set { zoomlevel = value; } }

        public int? Transparency { get { return transparency; } set { transparency = value; } }

        public static ImageOptions loadImagesOptionsFromFile(string xmlFileName)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                XDocument load = XDocument.Load(xmlFileName);
                var data = from query in load.Descendants("ImageOptions")
                           select new ImageOptions
                           {
                               Name = (string)query.Element("Name"),
                               Description = (string)query.Element("Description"),
                               NWLat=(double)query.Element("NWLat"),
                               NWLong=(double)query.Element("NWLong"),
                               SELat=(double)query.Element("SELat"),
                               SELong=(double)query.Element("SELong"),
                               SWLat=(double) query.Element("SWLat"),
                               SWLong=(double) query.Element("SWLong"),
                               NELat=(double)query.Element("NELat"),
                               NELong=(double)query.Element("NELong"),
                               //ZoomLevel=(int)query.Element("ZommLevel"),
                               //Transparency=(int)query.Element("TRansparency")
                           };
                if (data.Count<ImageOptions>() == 0)
                    throw new Exception("File not found or not in ImageOPtions.xml-Standars");
                else if(data.Count<ImageOptions>()>1)
                    throw new Exception("Multiple Results in ImageOPtionsFile");
                return data.First<ImageOptions>();
            }
        }
        
    }
}
