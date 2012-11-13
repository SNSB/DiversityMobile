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
using System.Reactive.Linq;
using DiversityPhone.Model;
using System.Linq;

namespace DiversityPhone.Services
{
    public class TestMapTransferService : IMapTransferService
    {

        public IObservable<Model.Map> downloadMap(string serverKey)
        {
            return Observable.Return(new Map() { Description = "TestMap"});
        }

        public IObservable<System.Collections.Generic.IEnumerable<string>> GetAvailableMaps(string searchString)
        {
            return Observable.Return(Enumerable.Repeat("TestMap", 1));
        }
    }
}
