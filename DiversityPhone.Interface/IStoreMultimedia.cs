using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DiversityPhone.Interface
{
    public interface IStoreMultimedia
    {
        string StoreMultimedia(string fileName, Stream data);
        Stream GetMultimedia(string fileName);

        void DeleteMultimedia(string uri);
        void ClearMultimedia();
    }
}
