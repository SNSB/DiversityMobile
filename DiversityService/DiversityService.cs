using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiversityService.Model;
using DiversityPhone.Model;
using DiversityORM;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Data.EntityClient;
using System.Globalization;
using DiversityService.Configuration;
using System.Threading.Tasks;



namespace DiversityService
{
    static class CredentialsExtensions
    {
        public static Diversity GetConnection(this UserCredentials This, string repository = null)
        {
            DiversityServiceConfiguration.Login l = This;
            var repo = Configuration.ServiceConfiguration.RepositoryByName(This.Repository);
            return new Diversity(l, repo.Server, repository ?? repo.Catalog);
        }
    }

    public partial class DiversityService : IDiversityService
    {       

        #region Get

        public IEnumerable<Term> GetStandardVocabulary(UserCredentials login)
        {

            IEnumerable<Term> linqTerms;
            using (var db = login.GetConnection())         
            {
                linqTerms =
                Enumerable.Concat(
                    db.Query<Term>("FROM [dbo].[DiversityMobile_TaxonomicGroups]() as Term")
                    .Select(t => { t.Source = DiversityPhone.Model.TermList.TaxonomicGroups; return t; }),
                    db.Query<Term>("FROM [dbo].[DiversityMobile_UnitRelationTypes]() as Term")
                    .Select(t => { t.Source = DiversityPhone.Model.TermList.RelationshipTypes; return t; })
                    ).ToList();
            }
            return linqTerms;

        }

        public IEnumerable<Project> GetProjectsForUser(UserCredentials login)
        {  
            if(string.IsNullOrWhiteSpace(login.Repository))
                return Enumerable.Empty<Project>();
            using (var db = login.GetConnection())
            {
                try
                {
                    return db.Query<Project>("FROM [dbo].[DiversityMobile_ProjectList] () AS [Project]")
                        .Select(p =>
                            {
                                p.DisplayText = p.DisplayText ?? "No Description";
                                return p;
                            })
                        .ToList(); //TODO Use credential DB
                }
                catch
                {
                    return Enumerable.Empty<Project>();
                }
            }            
        }

        public IEnumerable<AnalysisTaxonomicGroup> GetAnalysisTaxonomicGroupsForProject(int projectID, UserCredentials login)
        {
            using (var db = login.GetConnection())
            {
                var atgs = new Queue<AnalysisTaxonomicGroup>(analysisTaxonomicGroupsForProject(projectID,db));                    
                var flattened = new HashSet<AnalysisTaxonomicGroup>();
                var analyses = analysesForProject(projectID,db).ToLookup(an => an.AnalysisParentID);

                while (atgs.Any())
                {
                    var atg = atgs.Dequeue();                    
                    if (flattened.Add(atg)) //added just now -> queue children
                    {
                        if (analyses.Contains(atg.AnalysisID))
                            foreach (var child in analyses[atg.AnalysisID])
                                atgs.Enqueue(
                                    new AnalysisTaxonomicGroup()
                                    {
                                        AnalysisID = child.AnalysisID,
                                        TaxonomicGroup = atg.TaxonomicGroup
                                    });
                    }
                }
                return flattened;
            }
        }

        public IEnumerable<Model.Analysis> GetAnalysesForProject(int projectID, UserCredentials login)
        {
            using (var db = login.GetConnection())
            {
                var res = analysesForProject(projectID, db).ToList();
                return res;
            }
        }
        public IEnumerable<Model.AnalysisResult> GetAnalysisResultsForProject(int projectID, UserCredentials login)
        {
            using (var db = login.GetConnection())
            {
                return analysisResultsForProject(projectID, db).ToList();
            }
        }

        public UserProfile GetUserInfo(UserCredentials login)
        {
            try
            {
                using (var db = login.GetConnection())
                {
                    return db.Query<UserProfile>("FROM [DiversityMobile_UserInfo]() AS [UserProfile]").Single();
                }
            }
            catch
            {
                return null;
            }

        }

        private static readonly UserCredentials TNT_Login = new UserCredentials() { LoginName = "TNT", Password = "mu7idSwg", Repository="DiversityMobile" };

        public IEnumerable<Model.TaxonList> GetTaxonListsForUser(UserCredentials login)
        {
            List<Model.TaxonList> result = new List<TaxonList>();
            using (var db = login.GetConnection(CATALOG_DIVERSITYMOBILE))
            {
                result.AddRange(
                    taxonListsForUser(login.LoginName,db)
                    .Select(l => {l.IsPublicList = false; return l;})
                    );
            }
            var publicTaxa = ServiceConfiguration.PublicTaxa;
            using (var db = new DiversityORM.Diversity(publicTaxa.Login, publicTaxa.Server, publicTaxa.Catalog))
            {
                result.AddRange(
                    taxonListsForUser(db)
                    .Select(l => { l.IsPublicList = true; return l; })
                    );
            }
            return result;
        }
      
        public IEnumerable<TaxonName> DownloadTaxonList(TaxonList list, int page, UserCredentials login)
        {
            Diversity db;
            if (list.IsPublicList)
            {
                var taxa = ServiceConfiguration.PublicTaxa;
                db = new Diversity(taxa.Login, taxa.Server, taxa.Catalog);
            }
            else
                db = login.GetConnection(CATALOG_DIVERSITYMOBILE);

            return loadTablePaged<Model.TaxonName>(list.Table, page, db);                   
        }

        public IEnumerable<Model.Property> GetPropertiesForUser(UserCredentials login)
        {
            var propsForUser = propertyListsForUser(login).ToDictionary(pl => pl.PropertyID);
            
            using (var db = login.GetConnection())
            {
                return getProperties(db).Where(p => propsForUser.ContainsKey(p.PropertyID)).ToList();
            }            
        }

        public IEnumerable<Model.PropertyValue> DownloadPropertyNames(Property p, int page, UserCredentials login)
        {
            var propsForUser = propertyListsForUser(login).ToDictionary(pl => pl.PropertyID);
            PropertyList list;
            if (propsForUser.TryGetValue(p.PropertyID, out list))
            {
                return loadTablePaged<Model.PropertyValue>(list.Table, page, login.GetConnection(CATALOG_DIVERSITYMOBILE));                
            }
            else
                return Enumerable.Empty<Model.PropertyValue>();
        }

        public IEnumerable<Qualification> GetQualifications(UserCredentials login)
        {
            using (var db = login.GetConnection())
            {
                return getQualifications(db)
                    .Select(q => 
                        {
                            if(string.IsNullOrWhiteSpace(q.DisplayText))
                            {
                                q.DisplayText = "no qualifier";
                            }
                            return q;
                        })
                    .ToList();
            }
        }
       
        #endregion

        #region utility

        public IEnumerable<Repository> GetRepositories(UserCredentials login)
        {           

            List<Repository> result = new List<Repository>();

            Parallel.ForEach(Configuration.ServiceConfiguration.Repositories, repo =>
            {
                using (var ctx = new Diversity(login, repo.Server, repo.Catalog))
                {
                    try
                    {
                        ctx.OpenSharedConnection(); // validate Credentials
                        lock (result)
                        {
                            result.Add(new Repository() { Database = repo.Catalog, DisplayText = repo.name });
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            });

            return result;
        }
        #endregion

        
    }
}
