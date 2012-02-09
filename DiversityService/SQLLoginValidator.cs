using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IdentityModel.Selectors;
using System.Data.SqlClient;
using System.Configuration;
using System.IdentityModel.Tokens;

namespace DiversityService
{
    public class SQLLoginValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["DiversityMobileTemplate"];
                if(connectionString != null && connectionString.ConnectionString != null)
                {
                    var withuserinfo = String.Format(connectionString.ConnectionString, userName, password);
                    var conn = new SqlConnection(withuserinfo);

                    conn.Open();
                }
            }
            catch (Exception)
            {
                throw new SecurityTokenValidationException("Invalid Login");
            }
        }
    }
}
