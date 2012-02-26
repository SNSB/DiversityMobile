using DiversityService.Model;
using DiversityORM;
using System.Collections.Generic;


namespace DiversityService
{
    public partial class DiversityService
    {
        private static IEnumerable<AnalysisTaxonomicGroup> analysisTaxonomicGroupsForProject(int projectID, Diversity db)
        {
            return db.Query<AnalysisTaxonomicGroup>("FROM [DiversityMobile_AnalysisTaxonomicGroupsForProject](@0) AS [AnalysisTaxonomicGroup]", projectID);
        }

        private static IEnumerable<TaxonList> taxonListsForUser(string loginName, Diversity db)
        {
            return db.Query<TaxonList>("FROM [TaxonListsForUser](@0) AS [TaxonList]", loginName);
        }

        private static IEnumerable<Analysis> analysesForProject(int projectID, Diversity db)
        {
            return db.Query<Analysis>("FROM [DiversityMobile_AnalysisProjectList](@0) AS [Analysis]", projectID);
        }

        private static IEnumerable<AnalysisResult> analysisResultsForProject(int projectID, Diversity db)
        {
            return db.Query<AnalysisResult>("FROM [DiversityMobile_AnalysisResultForProject](@0) AS [AnalysisResult]", projectID);
        }

    }
}