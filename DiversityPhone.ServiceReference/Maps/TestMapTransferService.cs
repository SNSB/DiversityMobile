using DiversityPhone.Model;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace DiversityPhone.Services
{
    public class TestMapTransferService : IMapTransferService
    {
        public IObservable<Model.Map> downloadMap(string serverKey)
        {
            return Observable.Return(new Map() { Description = "TestMap" });
        }

        public IObservable<System.Collections.Generic.IEnumerable<string>> GetAvailableMaps(string searchString)
        {
            return Observable.Return(Enumerable.Repeat("TestMap", 1));
        }
    }
}