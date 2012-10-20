using System;
using System.Linq;
using System.Collections.Generic;
using DiversityPhone.Model;
using ReactiveUI;
using DiversityPhone.Messages;
using System.IO.IsolatedStorage;
using System.Data.Linq;
using System.IO;

namespace DiversityPhone.Services
{
    class MapDataContext : DataContext
    {
        public MapDataContext(string fileOrConnection)
            : base(fileOrConnection) 
        {
            if (!this.DatabaseExists())
                this.CreateDatabase();
        }

        
        public Table<Map> Maps; 
    }

    public class MapStorageService : IMapStorageService
    {
        private const string MapFolder = "Maps";
        private const string MapDB = "Maps.sdf";

        public MapStorageService()
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!iso.DirectoryExists(MapFolder))
                    iso.CreateDirectory(MapFolder);
            }
        }

        private string fileNameForMap(Map map)
        {
            return string.Format("{0}/{1}.png", MapFolder, map.ServerKey);
        }

        private MapDataContext getContext()
        {
            return new MapDataContext(string.Format("isostore:/{0}/{1}", MapFolder, MapDB));
        }

        public IList<Map> getAllMaps()
        {
            using (var ctx = getContext())
            {
                return ctx.Maps.ToList();
            }
        }

        public void addMap(Map map, System.IO.Stream mapContent)
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var filename = fileNameForMap(map);
                if (iso.FileExists(filename))
                    iso.DeleteFile(filename);

                using (var file = iso.CreateFile(filename))
                {
                    mapContent.CopyTo(file);
                }
            }

            using (var ctx = getContext())
            {
                ctx.Maps.InsertOnSubmit(map);
                ctx.SubmitChanges();
            }
        }

        public void clearMaps()
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                iso.DeleteDirectory(MapFolder);
                iso.CreateDirectory(MapFolder);
            }

            using (var ctx = getContext())
            {
                ctx.DeleteDatabase();
            }
        }

        public void deleteMap(Map map)
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var filename = fileNameForMap(map);
                if (iso.FileExists(filename))
                    iso.DeleteFile(filename);
            }

            using (var ctx = getContext())
            {
                ctx.Maps.DeleteOnSubmit(map);
                ctx.SubmitChanges();
            }
        }

        public System.IO.Stream loadMap(Map map)
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var filename = fileNameForMap(map);
                if (iso.FileExists(filename))
                {
                    using (var file = iso.OpenFile(filename, System.IO.FileMode.Open))
                    {
                        if (file.Length > int.MaxValue)
                            throw new ArgumentException("Map File too big");

                        var res = new MemoryStream((int)file.Length);
                        file.CopyTo(res);
                        return res;
                    }
                }
                else
                    return null;                    
            }
        }
    }        
}
