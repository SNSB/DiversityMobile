using System;
using System.Linq;
using ReactiveUI;
using DiversityPhone.MapMediaService;
using System.IO.IsolatedStorage;
using System.IO;
using DiversityPhone.Model;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Xml.Linq;
using System.Net;

namespace DiversityPhone.Services
{
    public class MapTransferService : IMapTransferService, IEnableLogger
    {

        private PhoneMediaServiceClient _mapinfo = new PhoneMediaServiceClient();
        

        private IObservable<GetMapListFilterCompletedEventArgs> GetMapsListCompletedObservable;
        private IObservable<GetMapUrlCompletedEventArgs> GetMapUrlCompletedObservable;
        private IObservable<GetXmlUrlCompletedEventArgs> GetXmlUrlCompletedObservable;

        public MapTransferService()            
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("Maps"))
                {
                    isoStore.CreateDirectory("Maps");
                }
            }

            GetMapsListCompletedObservable = Observable.FromEvent<EventHandler<GetMapListFilterCompletedEventArgs>, GetMapListFilterCompletedEventArgs>((a) => (s, args) => a(args), d => _mapinfo.GetMapListFilterCompleted += d, d => _mapinfo.GetMapListFilterCompleted -= d);
            GetMapUrlCompletedObservable = Observable.FromEvent<EventHandler<GetMapUrlCompletedEventArgs>, GetMapUrlCompletedEventArgs>((a) => (s, args) => a(args), d => _mapinfo.GetMapUrlCompleted += d, d => _mapinfo.GetMapUrlCompleted -= d);                
            GetXmlUrlCompletedObservable = Observable.FromEvent<EventHandler<GetXmlUrlCompletedEventArgs>, GetXmlUrlCompletedEventArgs>((a) => (s, args) => a(args), d => _mapinfo.GetXmlUrlCompleted += d, d => _mapinfo.GetXmlUrlCompleted -= d);               
        }

        public IObservable<IEnumerable<String>> GetAvailableMaps(String searchString)
        {
            var res = singleResultObservable(
                GetMapsListCompletedObservable
                .Where(args => Object.ReferenceEquals(searchString, args.UserState))
                .Select(args => args.Result as IEnumerable<string>));
            _mapinfo.GetMapListFilterAsync(searchString, searchString);
            return res;
        }

        public IObservable<Map> downloadMap(String serverKey)
        {
            var obs =
            Observable.Merge(
                GetMapUrlCompletedObservable
                .Where(args => Object.ReferenceEquals(serverKey, args.UserState))
                .Select(args => args.Result),
                GetXmlUrlCompletedObservable
                .Where(args => Object.ReferenceEquals(serverKey, args.UserState))
                .Select(args => args.Result))
                .SelectMany(uri =>
                    {
                        var request = WebRequest.CreateHttp(uri);
                        string credentials = Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes("snsb" + ":" + "maps"));
                        request.Headers["Authorization"] = "Basic " + credentials;

                        return request.GetResponseAsync().ToObservable();
                    })
                .Select(response =>
                    {
                        var http = response as HttpWebResponse;

                        if (http.StatusCode != HttpStatusCode.OK)
                            return null;

                        var isXML = http.ResponseUri.OriginalString.ToLower().EndsWith(".xml");

                        if (isXML)
                        {
                            return parseXMLtoMap(http.GetResponseStream());
                        }
                        else
                        {
                            String fileName = "Maps\\" + serverKey + ".png";

                            using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                if (isoStore.FileExists(fileName))
                                    isoStore.DeleteFile(fileName);

                                using (var isoFileStream = isoStore.CreateFile(fileName))
                                {
                                    response.GetResponseStream().CopyTo(isoFileStream);
                                    isoFileStream.Close();
                                }
                            }

                            return null;
                        }
                    })
                    .Window(2)
                    .Take(1)
                    .SelectMany(win => win.Where(res => res != null));

            _mapinfo.GetMapUrlAsync(serverKey, serverKey);
            _mapinfo.GetXmlUrlAsync(serverKey, serverKey);

            return obs;
        }

        private Map parseXMLtoMap(Stream contentStream)
        {            
            XDocument load = XDocument.Load(contentStream);
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
            if (data.Count() > 1)
                this.Log().Debug("Multiple Map XML Elements in content stream");

            return data.FirstOrDefault();        
        }

        private static IObservable<T> singleResultObservable<T>(IObservable<T> source)
        {
            var res = source
                .FirstAsync()
                .Replay(1);

            res.Connect();
            return res;
        }
    }
}
