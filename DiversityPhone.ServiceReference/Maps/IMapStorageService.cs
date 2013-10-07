using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
namespace DiversityPhone.Services
{
    public interface IMapStorageService
    {
        /// <summary>
        /// Loads the corresponding data of all Maps from the MapTable identified by the serverKey
        /// </summary>
        IList<Map> getAllMaps();

        /// <summary>
        /// Adds an entry with corresponding Data in the MapTable
        /// </summary>
        /// <returns></returns>
        IObservable<Unit> addMap(Map map, Stream mapContent);

        /// <summary>
        /// Deletes all Maps and removes the entries from the database
        /// </summary>
        void ClearMaps();

        /// <summary>
        /// Removes a Map in isolated storage and deletes the entry from the MapTable 
        /// </summary>
        /// <param name="uri"></param>
        void deleteMap(Map map);


        Stream loadMap(Map map);
    }
}
