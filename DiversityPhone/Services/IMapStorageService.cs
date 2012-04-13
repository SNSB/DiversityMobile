using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiversityPhone.Model;
using Svc = DiversityPhone.DiversityService;
namespace DiversityPhone.Services
{
    public interface IMapStorageService
    {


        /// <summary>
        /// Loads the corresponding data of all Maps from the MapTable identified by the serverKey
        /// </summary>
        IList<Map> getAllMaps();


        /// <summary>
        /// Loads the corresponding data of a Map from the MapTable identified by the serverKey
        /// </summary>
        /// <param name="serverkey"></param>
        Map getMapbyServerKey(String serverKey);


        /// <summary>
        /// Loads the corresponding data of a Map from the MapTable identified by the uri of the map in isolated storage
        /// </summary>
        /// <param name="uri"></param>
        Map getMapByURI(string uri);


        /// <summary>
        /// Loads the corresponding data of maps from the MapTable containig the point in WGS84 format specified by the parameters
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        IList<Map> getMapsForPoint(double latitude, double longitude);

        /// <summary>
        /// Checks if a map with the same name has already been downloaded
        /// </summary>
        /// <param name="serverKey"></param>
        /// <returns></returns>
        bool isPresent(String serverkey);

        /// <summary>
        /// Adds an entry with corresponding Data in the MapTable
        /// </summary>
        /// <returns></returns>
        void addOrUpdateMap(Map map);

        /// <summary>
        /// Deletes all Maps and derigesteres the entries in the database
        /// </summary>
        void deleteAllMaps();

        /// <summary>
        /// Removes a Map in isolated storage and deletes the entry from the MapTable 
        /// </summary>
        /// <param name="uri"></param>
        void deleteMap(Map map);
 

    }
}
