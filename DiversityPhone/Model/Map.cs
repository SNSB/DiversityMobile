using System;
using System.Data.Linq.Mapping;
using DiversityPhone.Services;
using System.Linq;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.IO;
using System.Windows;
using DiversityPhone.Model.Geometry;

namespace DiversityPhone.Model
{
    [Table]
    public class Map : IModifyable
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
        public double NWLat { get; set; }

        [Column]
        public double NWLong { get; set; }

        [Column]
        public double SELat { get; set; }

        [Column]
        public double SELong { get; set; }

        [Column]
        public double SWLat { get; set; }

        [Column]
        public double SWLong { get; set; }
        
        [Column]
        public double NELat { get; set; }

        [Column]
        public double NELong { get; set; }
       
        [Column(CanBeNull = true)]
        public int? Transparency{get;set;}

        [Column(CanBeNull = true)]
        public int? ZoomLevel{ get; set; }

      

        /// <summary>
        /// Tracks modifications to this Object.        
        /// </summary>
        [Column]
        public ModificationState ModificationState { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }

        public static IQueryOperations<Map> Operations
        {
            get;
            private set;
        }



        public Map()
        {
            this.ModificationState = ModificationState.New;
            this.LogUpdatedWhen = DateTime.Now;
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
                                       NWLat = (double)query.Element("NWLat"),
                                       NWLong = (double)query.Element("NWLong"),
                                       SELat = (double)query.Element("SELat"),
                                       SELong = (double)query.Element("SELong"),
                                       SWLat = (double)query.Element("SWLat"),
                                       SWLong = (double)query.Element("SWLong"),
                                       NELat = (double)query.Element("NELat"),
                                       NELong = (double)query.Element("NELong"),
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
