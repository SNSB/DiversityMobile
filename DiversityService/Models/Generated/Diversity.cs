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
        private const string CONNECTION_TEMPLATE_KEY = "ConnectionStringTemplate";


        public static string GetConnectionString(UserCredentials connection)
        {
            var cs = ConfigurationManager.ConnectionStrings[CONNECTION_TEMPLATE_KEY];
            if (cs != null)
            {
                var template = cs.ConnectionString;
                return string.Format(template,
                    connection.LoginName,
                    connection.Password,
                    connection.Repository ?? "DiversityCollection_Test");
            }
            return null;
        }
        public Diversity(UserCredentials connection)
            : base(GetConnectionString(connection), "System.Data.SqlClient")
        {

        }
    }
}
