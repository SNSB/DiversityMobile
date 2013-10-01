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
        private ICredentialsService CredentialsProvider;
        private PhoneMediaServiceClient MapService = new PhoneMediaServiceClient();


        private IObservable<EventPattern<GetMapListFilterCompletedEventArgs>> GetMapsListCompletedObservable;
        private IObservable<EventPattern<GetMapUrlCompletedEventArgs>> GetMapUrlCompletedObservable;
        private IObservable<EventPattern<GetXmlUrlCompletedEventArgs>> GetXmlUrlCompletedObservable;

        public MapTransferService(IMapStorageService storage, ICredentialsService creds)
        {
            MapStorage = storage;
            CredentialsProvider = creds;

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

            var map =
                GetXmlUrlCompletedObservable
                .FilterByUserState(dl)
                .Take(1)
                .PipeErrors()
                .Select(res => res.Result)
                .DownloadWithCredentials(CredentialsProvider)
                .Select(response => parseXMLtoMap(response));
            var image =
                GetMapUrlCompletedObservable
                .FilterByUserState(dl)
                .Take(1)
                .PipeErrors()
                .Select(res => res.Result)
                .DownloadWithCredentials(CredentialsProvider);

            var combined =
            Observable.CombineLatest(map, image,
                (m, i) => new { Map = m, ImageResponse = i })
                .Do(resps => resps.Map.ServerKey = serverKey)
                .SelectMany(resps =>
                                MapStorage.addMap(resps.Map, resps.ImageResponse.GetResponseStream())
                                    .Do(_ => resps.ImageResponse.Close())
                                    .Select(_ => resps.Map));
            var obs = combined
                .ReplayOnlyFirst();

            MapService.GetMapUrlAsync(serverKey, dl);
            MapService.GetXmlUrlAsync(serverKey, dl);

            return obs;
        }




        private Map parseXMLtoMap(WebResponse xmlResponse)
        {
            try
            {
                using (var contentStream = xmlResponse.GetResponseStream())
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
            finally
            {
                xmlResponse.Dispose();
            }
        }

    }
}
