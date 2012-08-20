using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using DiversityPhone.MapMediaService;
using System.IO.IsolatedStorage;
using System.IO;
using DiversityPhone.Model;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace DiversityPhone.Services
{
    public class MapTransferService : IMapTransferService
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



        #region Load Available Maps

        public IObservable<IEnumerable<String>> GetAvailableMaps(String searchString)
        {
            var res = singleResultObservable(
                GetMapsListCompletedObservable
                .Where(args => args.UserState == searchString)
                .Select(args => args.Result as IEnumerable<string>));
            _mapinfo.GetMapListFilterAsync(searchString, searchString);
            return res;
        }


        #endregion


        #region downLoadMap

        public IObservable<Map> downloadMap(String serverKey)
        {
            Observable.Merge(
                GetMapUrlCompletedObservable
                .Where(args => args.UserState == serverKey)
                .Select(args => args.Result),                
                GetXmlUrlCompletedObservable
                .Where(args => args.UserState == serverKey)
                .Select(args => args.Result))
                .SelectMany(uri => WebRequest.CreateHttp(uri).GetResponseAsync().ToObservable())
                .Window(2)
                .Select(results => 
                    {
                        
                    })

            _mapinfo.GetMapUrlAsync(serverKey, serverKey);
            _mapinfo.GetXmlUrlAsync(serverKey, serverKey);

           // _imageHttp = (HttpWebRequest)WebRequest.CreateHttp(mapUrl.Value);


            throw new NotImplementedException();
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
