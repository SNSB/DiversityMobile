namespace DiversityPhone.Services
{
    using System;

    public class ConnectivityService : IConnectivityService
    {
        public ConnectivityService()
        {
        }

        public ConnectionStatus CurrentStatus
        {
            get { throw new NotImplementedException(); }
        }
    }
}
