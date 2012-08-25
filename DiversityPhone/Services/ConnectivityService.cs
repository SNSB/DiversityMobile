namespace DiversityPhone.Services
{
    using System;    
    using Microsoft.Phone.Net.NetworkInformation;
    using System.Reactive.Disposables;
    using System.Reactive.Subjects;
    using Funq;
    using System.Reactive.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive;
    

    public class ConnectivityService : IConnectivityService
    {
        private IObservable<ConnectionStatus> status;

        public ConnectivityService()
        {

            

            var s =Observable.Merge(
                            Observable.FromEventPattern<object, EventArgs>(typeof(System.Net.NetworkInformation.NetworkChange), "NetworkAddressChanged") 
                                .Select( _ => 0L),
                            Observable.Interval(TimeSpan.FromSeconds(30))
                   )
                        .StartWith(0)
                        .Select(_ =>
                            {
                                if (NetworkInterface.GetIsNetworkAvailable())
                                {
                                    var it = NetworkInterface.NetworkInterfaceType;

                                    if (it == NetworkInterfaceType.Wireless80211 || it == NetworkInterfaceType.Ethernet)
                                        return ConnectionStatus.Wifi;
                                    if (it == NetworkInterfaceType.MobileBroadbandGsm || it == NetworkInterfaceType.MobileBroadbandCdma)
                                        return ConnectionStatus.MobileBroadband;
                                }
                                return ConnectionStatus.None;
                            })                        
                        .Publish();
            s.Connect();
            status = s;
        }

        public IObservable<ConnectionStatus> Status()
        {
            return status.AsObservable();
        }
    }
}
