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

    public enum ConnectionStatus
    {
        Wifi,
        MobileBroadband,
        None
    }

    public interface IConnectivityService
    {
        IObservable<ConnectionStatus> Status();
        void ForceUpdate();
    }

    public static class ConnectivityMixin
    {
        public static IObservable<bool> WifiAvailable(this IConnectivityService svc)
        {
            return svc.Status().Select(s => s == ConnectionStatus.Wifi);
        }
    }


    public class ConnectivityService : IConnectivityService
    {
        private IObservable<ConnectionStatus> status;

        private ISubject<Unit> updateSubject;

        public ConnectivityService()
        {
            updateSubject = new Subject<Unit>();

            status =
            Observable.FromEventPattern<NetworkNotificationEventArgs>(h => DeviceNetworkInformation.NetworkAvailabilityChanged += h, h => DeviceNetworkInformation.NetworkAvailabilityChanged -= h)
                .Select(_ => 0L)
                .Merge(Observable.Interval(TimeSpan.FromSeconds(10), ThreadPoolScheduler.Instance))
                .Merge(updateSubject.Select(_ => 0L))
                .StartWith(0L)
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
                .DistinctUntilChanged()
                .Replay(1)
                .RefCount();
        }

        public IObservable<ConnectionStatus> Status()
        {
            return status;
        }

        public void ForceUpdate()
        {
            updateSubject.OnNext(Unit.Default);
        }
    }
}
