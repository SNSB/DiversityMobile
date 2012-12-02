using System;
using System.Linq;
using ReactiveUI;
using DiversityPhone.MapService;
using System.IO.IsolatedStorage;
using System.IO;
using DiversityPhone.Model;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Xml.Linq;
using System.Net;
using Funq;
using System.Reactive.Threading.Tasks;

namespace DiversityPhone.Services
{
    public partial class MapTransferService : IMapTransferService, IEnableLogger
    {
        private IMapStorageService MapStorage;
        private PhoneMediaServiceClient MapService = new PhoneMediaServiceClient();
        

        private IObservable<GetMapListFilterCompletedEventArgs> GetMapsListCompletedObservable;
        private IObservable<GetMapUrlCompletedEventArgs> GetMapUrlCompletedObservable;
        private IObservable<GetXmlUrlCompletedEventArgs> GetXmlUrlCompletedObservable;

        public MapTransferService(Container ioc)            
        {
            MapStorage = ioc.Resolve<IMapStorageService>();

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("Maps"))
                {
                    isoStore.CreateDirectory("Maps");
                }
            }

            GetMapsListCompletedObservable = Observable.FromEvent<EventHandler<GetMapListFilterCompletedEventArgs>, GetMapListFilterCompletedEventArgs>((a) => (s, args) => a(args), d => MapService.GetMapListFilterCompleted += d, d => MapService.GetMapListFilterCompleted -= d);
            GetMapUrlCompletedObservable = Observable.FromEvent<EventHandler<GetMapUrlCompletedEventArgs>, GetMapUrlCompletedEventArgs>((a) => (s, args) => a(args), d => MapService.GetMapUrlCompleted += d, d => MapService.GetMapUrlCompleted -= d);                
            GetXmlUrlCompletedObservable = Observable.FromEvent<EventHandler<GetXmlUrlCompletedEventArgs>, GetXmlUrlCompletedEventArgs>((a) => (s, args) => a(args), d => MapService.GetXmlUrlCompleted += d, d => MapService.GetXmlUrlCompleted -= d);               
        }

        public IObservable<IEnumerable<String>> GetAvailableMaps(String searchString)
        {
            var res = singleResultObservable(
                GetMapsListCompletedObservable
                .Where(args => Object.ReferenceEquals(searchString, args.UserState))
                .Select(args => args.Result as IEnumerable<string>));
            MapService.GetMapListFilterAsync(searchString, searchString);
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
                .Select( response =>
                    {
                        var http = response as HttpWebResponse;

                        if (http.StatusCode != HttpStatusCode.OK)
                            return null;

                        String fileName = "Maps\\" + serverKey + ".png";

                        var isXML = http.ResponseUri.OriginalString.ToLower().EndsWith(".xml");

                        if (isXML)
                        {
                            var map = parseXMLtoMap(http.GetResponseStream());
                            if (map != null)
                            {
                                map.ServerKey = serverKey;
                            }
                            return map as object;
                        }
                        else
                        {
                            return http.GetResponseStream() as object;
                        }
                    })
                    .Buffer(2)
                    .Take(1)
                    .SelectMany(win =>
                        {
                            var map = win.Where(i => i is Map).FirstOrDefault() as Map;
                            var stream = win.Where(i => i is Stream).FirstOrDefault() as Stream;

                            return Observable.Start(() =>
                                {
                                    MapStorage.addMap(map, stream);
                                    return map;
                                });
                        })                    
                    .Publish();
            obs.Connect();
            
            MapService.GetMapUrlAsync(serverKey, serverKey);
            MapService.GetXmlUrlAsync(serverKey, serverKey);

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
