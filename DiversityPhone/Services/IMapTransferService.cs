using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiversityPhone.Model;
using Svc = DiversityPhone.DiversityService;

namespace DiversityPhone.Services
{
    public interface IMapTransferService:IMapStorageService
    {
        /// <summary>
        /// Downloads a Map and saves it to isolated storage, and registeres it in the Map Database
        /// </summary>        
        /// <param name="MapName">serverKey: String which is used to identify the map in the mapServer</param>
        IObservable<Map> downloadMap(String serverKey);
        

        /// <summary>
        /// Retrieves ServerKeys from the mapserver that conform to the query string from the repository
        /// </summary>
        /// <param name="searchText">substring of the mapNames to search. Minimum Length is 3</param>
        /// <returns></returns>
        IObservable<IEnumerable<String>> GetAvailableMaps(String searchString);

        

    }
}
