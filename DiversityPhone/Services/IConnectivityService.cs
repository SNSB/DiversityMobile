using System;
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
            throw new NotImplementedException();
        }
    }
}
