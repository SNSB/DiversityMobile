using System;
using System.Data.Linq.Mapping;
using DiversityPhone.Services;
using System.Linq;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.IO;

namespace DiversityPhone.Model
{
    [Table]
    public class Map:IModifyable
    {

        [Column(IsPrimaryKey = true)]
        public String ServerKey { get; set; }

        [Column]
        public String Uri { get; set; }

        [Column]
        public String Name { get; set; }

        [Column]
        public String Description { get; set; }

        [Column]
        public double LatitudeNorth { get; set; }

        [Column]
        public double LatitudeSouth { get; set; }

        [Column]
        public double LongitudeWest { get; set; }

        [Column]
        public double LongitudeEast { get; set; }

        [Column(CanBeNull = true)]
        public int? Transparency{get;set;}

        [Column(CanBeNull = true)]
        public int? ZoomLevel{ get; set; }

        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull = true)]
        public bool? ModificationState { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }

        public static IQueryOperations<Map> Operations
        {
            get;
            private set;
        }
    
        public Map()
        {            
            this.ModificationState= null;
            this.LogUpdatedWhen = DateTime.Now;

            Operations = new QueryOperations<Map>(
                //Smallerthan
                        (q, map) => q.Where(row => row.LatitudeNorth < map.LatitudeNorth),
                //Equals
                        (q, map) => q.Where(row => row.ServerKey == map.ServerKey),
                //Orderby
                        (q) => from map in q
                               orderby map.LatitudeNorth, map.LongitudeWest
                               select map,
                //FreeKey
                        (q, cep) =>
                        {
                            //Not Applicable
                        });

        }

        public static bool isOnMap(Map map, double latitude, double longitude)
        {
      
            if (map.LatitudeNorth > latitude || map.LatitudeSouth < latitude)
                return false;
            if (map.LongitudeWest < longitude || map.LongitudeEast > longitude)
                return false;
            return true;
        }

        public static Map loadMapParameterFromFile(string xmlFileName)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.FileExists(xmlFileName))
                    throw new FileNotFoundException(xmlFileName + " not found");
                try
                {
                    using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(xmlFileName, FileMode.Open, isoStore))
                    {
                        XDocument load = XDocument.Load(isoStream);
                        var data = from query in load.Descendants("ImageOptions")
                                   select new Map
                                   {
                                       Name = (string)query.Element("Name"),
                                       Description = (string)query.Element("Description"),
                                       LatitudeNorth = (double)query.Element("NWLat"),
                                       LongitudeWest = (double)query.Element("NWLong"),
                                       LatitudeSouth = (double)query.Element("SELat"),
                                       LongitudeEast = (double)query.Element("SELong"),
                                       ZoomLevel = (int?)query.Element("ZommLevel"),
                                       Transparency = (int?)query.Element("Transparency")
                                   };
                        if (data.Count<Map>() == 0)
                            throw new Exception("File not found or not in ImageOptions.xml-Standars");
                        else if (data.Count<Map>() > 1)
                            throw new Exception("Multiple Results in ImageOptionsFile");
                        return data.First<Map>();
                    }
                }

                catch (Exception e)
                {
                    Map i = new Map();
                    i.Name = e.Message;
                    i.Description = "XML-Parse-Error";
                    return i;
                }
            }
        }

    }
}
