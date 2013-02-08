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
using System.Reactive.Threading.Tasks;
using System.Reactive;

namespace DiversityPhone.Services
{
    public partial class MapTransferService : IMapTransferService, IEnableLogger
    {
        private IMapStorageService MapStorage;
        private PhoneMediaServiceClient MapService = new PhoneMediaServiceClient();


        private IObservable<EventPattern<GetMapListFilterCompletedEventArgs>> GetMapsListCompletedObservable;
        private IObservable<EventPattern<GetMapUrlCompletedEventArgs>> GetMapUrlCompletedObservable;
        private IObservable<EventPattern<GetXmlUrlCompletedEventArgs>> GetXmlUrlCompletedObservable;

        public MapTransferService(IMapStorageService storage)
        {
            MapStorage = storage;

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("Maps"))
                {
                    isoStore.CreateDirectory("Maps");
                }
            }

            GetMapsListCompletedObservable = Observable.FromEventPattern<GetMapListFilterCompletedEventArgs>(d => MapService.GetMapListFilterCompleted += d, d => MapService.GetMapListFilterCompleted -= d);
            GetMapUrlCompletedObservable = Observable.FromEventPattern<GetMapUrlCompletedEventArgs>(d => MapService.GetMapUrlCompleted += d, d => MapService.GetMapUrlCompleted -= d);
            GetXmlUrlCompletedObservable = Observable.FromEventPattern<GetXmlUrlCompletedEventArgs>(d => MapService.GetXmlUrlCompleted += d, d => MapService.GetXmlUrlCompleted -= d);
        }

        public IObservable<IEnumerable<String>> GetAvailableMaps(String searchString)
        {
            object request = new object();
            var res = GetMapsListCompletedObservable
                .MakeObservableServiceResultSingle(request)
                .Select(args => args.Result as IEnumerable<string>);
            MapService.GetMapListFilterAsync(searchString, request);
            return res;
        }

        public IObservable<Map> downloadMap(String serverKey)
        {
            object dl = new object();
            var obs =
            Observable.Merge(
                GetMapUrlCompletedObservable
                .MakeObservableServiceResultSingle(dl)
                .Select(args => args.Result),
                GetXmlUrlCompletedObservable
                .MakeObservableServiceResultSingle(dl)
                .Select(args => args.Result))
                .SelectMany(uri =>
                    {
                        var request = WebRequest.CreateHttp(uri);
                        string credentials = Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes("snsb" + ":" + "maps"));
                        request.Headers["Authorization"] = "Basic " + credentials;

                        return Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.EndGetResponse)();
                    })
                .Select(response =>
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

            MapService.GetMapUrlAsync(serverKey, dl);
            MapService.GetXmlUrlAsync(serverKey, dl);

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

    }
}
