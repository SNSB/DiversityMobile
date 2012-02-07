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
using System.IO;

namespace DiversityPhone.Model
{
    //Class for savin Coordinates of Maps provided by the Map-Server in an XML-File
    public class MapParameter
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

        public static MapParameter loadMapParameterFromFile(string xmlFileName)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.FileExists(xmlFileName))
                    throw new FileNotFoundException(xmlFileName+ " not found");
                try
                {
                    using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(xmlFileName, FileMode.Open, isoStore))
                    {
                        XDocument load = XDocument.Load(isoStream);
                        var data = from query in load.Descendants("ImageOptions")
                                   select new MapParameter
                                   {
                                       Name = (string)query.Element("Name"),
                                       Description = (string)query.Element("Description"),
                                       NWLat = (double)query.Element("NWLat"),
                                       NWLong = (double)query.Element("NWLong"),
                                       SELat = (double)query.Element("SELat"),
                                       SELong = (double)query.Element("SELong"),
                                       SWLat = (double)query.Element("SWLat"),
                                       SWLong = (double)query.Element("SWLong"),
                                       NELat = (double)query.Element("NELat"),
                                       NELong = (double)query.Element("NELong"),
                                       //ZoomLevel=(int)query.Element("ZommLevel"),
                                       //Transparency=(int)query.Element("TRansparency")
                                   };
                        if (data.Count<MapParameter>() == 0)
                            throw new Exception("File not found or not in ImageOptions.xml-Standars");
                        else if (data.Count<MapParameter>() > 1)
                            throw new Exception("Multiple Results in ImageOPtionsFile");
                        return data.First<MapParameter>();
                    }
                }

                catch (Exception e) { 
                    MapParameter i= new MapParameter();
                    i.Name = e.Message;
                    i.Description = "XML-Parse-Error";
                    return i;
                }
            }
        }
        
    }
}
