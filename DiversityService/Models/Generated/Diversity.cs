using Config = DiversityServiceConfiguration;

namespace DiversityORM
{
    public partial class Diversity
    {
        private const string CONNECTION_STRING_TEMPLATE = "Data Source={0},{1};Initial Catalog={4};User ID={2};Password={3}";

        public static string GetConnectionString(Config.Login connection, Config.Server server, string repository)
        {          

            return string.Format(CONNECTION_STRING_TEMPLATE,
                server.address,
                server.port,
                connection.user,
                connection.password,
                repository);
            
        }
        public Diversity(Config.Login connection, Config.Server server, string repository)
            : base(GetConnectionString(connection, server, repository), "System.Data.SqlClient")
        {

        }
    }
}
