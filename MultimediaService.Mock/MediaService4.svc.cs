using System;

namespace DiversityMediaService4
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Klassennamen "Service1" sowohl im Code als auch in der SVC- und der Konfigurationsdatei ändern.
    public class MediaService4 : IMediaService4
    {
        public string BeginTransaction(
            string rowGuid,   	// rowGuid of the image 
            string fileName,  	// complete file name: <GUID>.jpg/.wav/.mpg
            string type,      	// "audio", "video", "photograph"
            float latitude,  	// location data 1 from GPS
            float longitude, 	// location data 2 from GPS
            float altitude,  	// location data 3 from GPS
            string author,    	// author
            string timestamp,   // time of recording (date, time, timezone)   
            int projectId)      // project ID (integer number)   
        {
            return string.Empty;
        }

        public void PrepareTransfer(
            string rowGuid,   	// rowGuid of the image 
            string fileName,  	// complete file name: <GUID>.jpg/.wav/.mpg
            string type,      	// "Audio", "Video", "Image"
            float latitude,  	// location data 1 from GPS
            float longitude, 	// location data 2 from GPS
            float altitude,  	// location data 3 from GPS
            string author,    	// author
            string timestamp,   // time of recording (date, time, timezone)   
            int projectId)      // project ID (integer number)   
        {
          
        }

        public string EncodeFile(byte[] data)   // media data, encoded as byte-array
        {
            return string.Empty;
        }

        public string Commit()
        {
            return string.Empty;
        }

        public void Rollback()
        {
            
        }

        public string Submit(
            string rowGuid,   	// rowGuid of the image 
            string fileName,  	// complete file name: <GUID>.jpg/.wav/.mpg
            string type,      	// "Audio", "Video", "Image"
            float latitude,  	// location data 1 from GPS
            float longitude, 	// location data 2 from GPS
            float altitude,  	// location data 3 from GPS
            string author,    	// author
            string timestamp,   // time of recording (date, time, timezone)   
            int projectId,      // project ID (integer number)  
            byte[] data)        // transmitted data
        {
            return string.Format("http://{0}",Guid.NewGuid().ToString());
        }
    }
}
