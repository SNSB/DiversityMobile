using System.Collections.Generic;
using DiversityPhone.Model;
namespace DiversityPhone.Services
{
    public interface IMapStorageService
    {
        /// <summary>
        /// Loads the corresponding data of all Maps from the MapTable identified by the serverKey
        /// </summary>
        IList<Map> getAllMaps();

        /// <summary>
        /// Checks if a map with the same name has already been downloaded
        /// </summary>
        /// <param name="serverKey"></param>
        /// <returns></returns>
        bool isPresent(string serverkey);

        /// <summary>
        /// Adds an entry with corresponding Data in the MapTable
        /// </summary>
        /// <returns></returns>
        void addMap(Map map);

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
