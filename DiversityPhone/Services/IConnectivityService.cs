namespace DiversityPhone.Services
{
    public interface IConnectivityService
    {
        ConnectionStatus CurrentStatus { get; }
    }
}
