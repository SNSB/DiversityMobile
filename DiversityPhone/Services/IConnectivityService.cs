using System;
using System.Reactive.Linq;
namespace DiversityPhone.Services
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
    }

    public static class ConnectivityMixin
    {
        public static IObservable<bool> WifiAvailable(this IConnectivityService svc)
        {
            return svc.Status().Select(s => s == ConnectionStatus.Wifi);
        }
    }
}
