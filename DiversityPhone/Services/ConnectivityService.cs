namespace DiversityPhone.Services
{
    using System;    
    using Microsoft.Phone.Net.NetworkInformation;
    using System.Reactive.Disposables;
    using System.Reactive.Subjects;
    using Funq;
    using System.Reactive.Linq;
    using System.Reactive.Concurrency;

    public class ConnectivityService : IConnectivityService
    {       
        private ISubject<ConnectionStatus> status_subject = new BehaviorSubject<ConnectionStatus>(ConnectionStatus.None);

        public ConnectivityService()
        {
            Observable.FromEventPattern<System.Net.NetworkInformation.NetworkChange, EventArgs>(typeof(System.Net.NetworkInformation.NetworkChange), "NetworkAdressChanged")
                        .Select(_ =>
                            {
                                if (NetworkInterface.GetIsNetworkAvailable())
                                {
                                    var it = NetworkInterface.NetworkInterfaceType;

                                    if (it == NetworkInterfaceType.Wireless80211)
                                        return ConnectionStatus.Wifi;
                                    if (it == NetworkInterfaceType.MobileBroadbandGsm || it == NetworkInterfaceType.MobileBroadbandCdma)
                                        return ConnectionStatus.MobileBroadband;
                                }
                                return ConnectionStatus.None;
                            }).Subscribe(status_subject);
        }

        public IObservable<ConnectionStatus> Status()
        {
            return status_subject.AsObservable();
        }
    }
}
