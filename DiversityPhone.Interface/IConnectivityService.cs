using System;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;

namespace DiversityPhone.Interface
{
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
            Contract.Requires(svc != null);

            return svc.Status().Select(s => s == ConnectionStatus.Wifi);
        }
    }

}
