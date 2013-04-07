using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace DiversityMediaService4
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Schnittstellennamen "IService1" sowohl im Code als auch in der Konfigurationsdatei ändern.
    [ServiceContract]
    public interface IMediaService4
    {

        [OperationContract]
        string BeginTransaction(
            string rowGuid,   		    // rowGuid of the image (not used)
            string fileName,  		    // complete file name: <GUID>.jpg/.wav/.mpg
            string type,      		    // "photograph", "audio", "video"
            float latitude,  		    // location data 1 from GPS
            float longitude, 		    // location data 2 from GPS
            float altitude,  		    // location data 3 from GPS
            string author,    		    // author
            string timestamp, 	        // time of recording (date, time, timezone)  
            int projectId); 	        // project ID (integer number)  

        [OperationContract]
        string EncodeFile(byte[] data);   // media data, encoded as byte-array                             

        [OperationContract]
        string Commit();

        [OperationContract]
        void Rollback();

        [OperationContract]
        string Submit(
            string rowGuid,   		    // rowGuid of the image (not used)
            string fileName,  		    // complete file name: <GUID>.jpg/.wav/.mpg
            string type,      		    // "photograph", "audio", "video"
            float latitude,  		    // location data 1 from GPS
            float longitude, 		    // location data 2 from GPS
            float altitude,  		    // location data 3 from GPS
            string author,    		    // author
            string timestamp, 	        // time of recording (date, time, timezone)  
            int projectId,  	        // project ID (integer number)  
            byte[] data);               // media data, encoded as byte-array    

    }


    // Verwenden Sie einen Datenvertrag, wie im folgenden Beispiel dargestellt, um Dienstvorgängen zusammengesetzte Typen hinzuzufügen.
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}
