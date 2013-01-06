using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiversityService.Model;
using DiversityORM;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Data.EntityClient;
using System.Globalization;



namespace DiversityService
{
    public partial class DiversityService : IDiversityService
    {       

        #region Get

        public IEnumerable<Term> GetStandardVocabulary(UserCredentials login)
        {

            IEnumerable<Term> linqTerms;
            using (var db = new Diversity(login,Diversity.SERVER_COLLECTION))         
            {
                linqTerms =
                Enumerable.Concat(
                    db.Query<Term>("FROM [dbo].[DiversityMobile_TaxonomicGroups]() as Term")
                    .Select(t => { t.Source = TermList.TaxonomicGroups; return t;}),
                    db.Query<Term>("FROM [dbo].[DiversityMobile_UnitRelationTypes]() as Term")
                    .Select(t => { t.Source = TermList.RelationshipTypes; return t; })
                    ).ToList();
            }
            return linqTerms;

        }

        public IEnumerable<Project> GetProjectsForUser(UserCredentials login)
        {  
            if(string.IsNullOrWhiteSpace(login.Repository))
                return Enumerable.Empty<Project>();
            using (var db = new DiversityORM.Diversity(login))
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
            using (var db = new DiversityORM.Diversity(login))
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
            using (var db = new DiversityORM.Diversity(login))
            {
                var res = analysesForProject(projectID, db).ToList();
                return res;
            }
        }
        public IEnumerable<Model.AnalysisResult> GetAnalysisResultsForProject(int projectID, UserCredentials login)
        {
            using (var db = new DiversityORM.Diversity(login))
            {
                return analysisResultsForProject(projectID, db).ToList();
            }
        }

        public UserProfile GetUserInfo(UserCredentials login)
        {
            try
            {
                using (var db = new DiversityORM.Diversity(login))
                {
                    return db.Query<UserProfile>("FROM [DiversityMobile_UserInfo]() AS [UserProfile]").Single();
                }
            }
            catch
            {
                return null;
            }

        }

        private static readonly UserCredentials TNT_Login = new UserCredentials() { LoginName = "TNT", Password = "mu7idSwg",Repository="DiversityMobile" };

        public IEnumerable<Model.TaxonList> GetTaxonListsForUser(UserCredentials login)
        {
            List<Model.TaxonList> result = new List<TaxonList>();
            using (var db = new DiversityORM.Diversity(login, CATALOG_DIVERSITYMOBILE))
            {
                result.AddRange(
                    taxonListsForUser(login.LoginName,db)
                    .Select(l => {l.IsPublicList = false; return l;})
                    );
            }
            using (var db = new DiversityORM.Diversity(TNT_Login,Diversity.SERVER_TNT))
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
                db = new Diversity(TNT_Login, Diversity.SERVER_TNT);
            else
                db = new Diversity(login, CATALOG_DIVERSITYMOBILE);

            return loadTablePaged<Model.TaxonName>(list.Table, page, db);                   
        }

        public IEnumerable<Model.Property> GetPropertiesForUser(UserCredentials login)
        {
            var propsForUser = propertyListsForUser(login).ToDictionary(pl => pl.PropertyID);
            
            using (var db = new DiversityORM.Diversity(login))
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
                return loadTablePaged<Model.PropertyValue>(list.Table, page, new Diversity(login, CATALOG_DIVERSITYMOBILE));                
            }
            else
                return Enumerable.Empty<Model.PropertyValue>();
        }

        public IEnumerable<Qualification> GetQualifications(UserCredentials login)
        {
            using (var db = new Diversity(login))
            {
                return getQualifications(db)
                    .Select(q => 
                        {
                            if(string.IsNullOrWhiteSpace(q.DisplayText))
                            {
                                q.DisplayText = "no qualification";
                            }
                            return q;
                        })
                    .ToList();
            }
        }
       
        #endregion

        #region utility
        public static readonly Repository[] Repositories = new Repository[]
            {
                new Repository()
                { 
                    DisplayText = "Test",
                    Database = "DiversityCollection_Test"
                },
                /*new Repository() // In München funktionen noch nicht implementiert
                {
                    DisplayText="DiversityCollection",
                    Database="DiversityCollection",
                },*/
                 new Repository() 
                {
                    DisplayText="DiversityCollection Monitoring",
                    Database="DiversityCollection_Monitoring",
                },
            };

        public IEnumerable<Repository> GetRepositories(UserCredentials login)
        {
            List<Repository> result = new List<Repository>();

            foreach (var repo in Repositories)
            {
                login.Repository = repo.Database;
                using (var ctx = new Diversity(login))
                {
                    try
                    {
                        ctx.OpenSharedConnection(); // validate Credentials
                        result.Add(repo);
                    }
                    catch (Exception)
                    {                        
                    }
                }  
            }

            return result;
        }
        #endregion

        
    }
}
