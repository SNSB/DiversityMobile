using System.IO;

namespace DiversityPhone.Interface
{
    public interface IStoreMultimedia
    {
        /// <summary>
        /// Stores the data for one Multimedia File 
        /// </summary>
        /// <param name="fileNameHint">Desired File Name</param>
        /// <param name="data"></param>
        /// <returns>A string URI identifying the multimedia item for later retrieval</returns>
        string StoreMultimedia(string fileNameHint, Stream data);
        
        Stream GetMultimedia(string URI);

        void DeleteMultimedia(string URI);

        void ClearAllMultimedia();
    }
}
