using DiversityService.Model;
using DiversityORM;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;


namespace DiversityService
{
    public partial class DiversityService
    {
        private const string CATALOG_DIVERSITYMOBILE = "DiversityMobile";

        private static IEnumerable<AnalysisTaxonomicGroup> analysisTaxonomicGroupsForProject(int projectID, Diversity db)
        {
            return db.Query<AnalysisTaxonomicGroup>("FROM [DiversityMobile_AnalysisTaxonomicGroupsForProject](@0) AS [AnalysisTaxonomicGroup]", projectID);
        }

        private static IEnumerable<TaxonList> taxonListsForUser(string loginName, Diversity db)
        {
            return db.Query<TaxonList>("FROM [TaxonListsForUser](@0) AS [TaxonList]", loginName);
        }

        private static IEnumerable<TaxonList> taxonListsForUser(Diversity db)
        {
            return db.Query<TaxonList>("FROM [TaxonListsForUser]() AS [TaxonList]");
        }

        private static IEnumerable<Analysis> analysesForProject(int projectID, Diversity db)
        {
            return db.Query<Analysis>("FROM [DiversityMobile_AnalysisProjectList](@0) AS [Analysis]", projectID);
        }

        private static IEnumerable<AnalysisResult> analysisResultsForProject(int projectID, Diversity db)
        {
            return db.Query<AnalysisResult>("FROM [DiversityMobile_AnalysisResultForProject](@0) AS [AnalysisResult]", projectID);
        }

        private static IEnumerable<PropertyList> propertyListsForUser(UserCredentials login)
        {
            return new Diversity(login,CATALOG_DIVERSITYMOBILE).Query<PropertyList>("FROM [TermsListsForUser](@0) AS [PropertyList]", login.LoginName);
        }

        private static IEnumerable<Property> getProperties(Diversity db)
        {
            return db.Query<Property>("FROM [Property] AS [Property]");
        }

        private static IEnumerable<T> loadTablePaged<T>(string table, int page, Diversity db)
        {
            //TODO Improve SQL Sanitation
            if (table.Contains(";") ||
                table.Contains("'") ||
                table.Contains("\""))
                return Enumerable.Empty<T>();  //SQL Injection ?

            var sql = PetaPoco.Sql.Builder
                .From(String.Format("[dbo].[{0}] AS [{1}]", table, typeof(T).Name))
                .SQL;
            return db.Page<T>(page, 1000, sql).Items;
        }       

    }
}