using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiversityService.Model;
using System.Configuration;

namespace DiversityORM
{
    public partial class Diversity 
    {
        private const string CONNECTION_STRING_TEMPLATE = "Data Source={0},{1};Initial Catalog={4};User ID={2};Password={3}";
        public static readonly ServerInfo SERVER_COLLECTION = new ServerInfo("141.84.65.107", 5432);
        public static readonly ServerInfo SERVER_TNT = new ServerInfo("tnt.diversityworkbench.de", 5432);


        public static string GetConnectionString(UserCredentials connection, ServerInfo server, string repository = null)
        {        
            return string.Format(CONNECTION_STRING_TEMPLATE,
                server.Server,
                server.Port,
                connection.LoginName,
                connection.Password,
                repository ?? connection.Repository);
            
        }

        public Diversity(UserCredentials connection, string repository = null)
            : this(connection, SERVER_COLLECTION, repository)
        {

        }

        public Diversity(UserCredentials connection, ServerInfo server, string repository = null)
            : base(GetConnectionString(connection, server, repository), "System.Data.SqlClient")
        {

        }
    }
}
