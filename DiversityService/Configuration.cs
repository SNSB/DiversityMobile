using System.Collections.Generic;
using System.Linq;
using Model = DiversityPhone.Model;

namespace DiversityServiceConfiguration
{
    public partial class Login
    {
        public static implicit operator Login(Model.UserCredentials creds)
        {
            return new Login()
            {
                user = creds.LoginName,
                password = creds.Password
            };
        }
    }
}

namespace DiversityService.Configuration
{
    using DiversityServiceConfiguration;



    class ServiceConfiguration
    {
        static ServiceConfiguration _instance;
        static Configuration _cfg;
        static IEnumerable<Repository> _repos;
        static PublicTaxonConfig _public_taxa;

        public static IEnumerable<Repository> Repositories
        {
            get
            {
                return _repos;
            }
        }

        public static PublicTaxonConfig PublicTaxa
        {
            get
            {
                return _public_taxa;
            }
        }

        public static Repository RepositoryByName(string name)
        {
            return Repositories.FirstOrDefault(r => r.name == name);
        }

        private ServiceConfiguration()
        {
            _cfg = Configuration.Load(System.Web.Hosting.HostingEnvironment.MapPath(@"~\DiversityServiceConfiguration.xml"));
            _repos = _cfg.Repositories.Repository;
            _public_taxa = _cfg.PublicTaxonConfig;
        }

        static ServiceConfiguration()
        {
            _instance = new ServiceConfiguration();
        }
    }
}
