
namespace DiversityService.Model
{
    public class ServerInfo
    {
        public ServerInfo(string server, int port)
        {
            Server = server;
            Port = port;
        }

        public string Server { get; private set; }

        public int Port { get; private set; }
    }
}
