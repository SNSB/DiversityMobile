using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiversityService.Model;
using DiversityMobile;


namespace DiversityService
{
    public class DiversityService : IDiversityService
    {

        public IEnumerable<Model.Project> GetProjectsForUser(Model.UserProfile user)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AnalysisResult> GetAnalysisResults(IList<int> analysisKeys)
        {
            return null; //TODO
        }

        public IEnumerable<AnalysisResult> GetAnalysisTaxonomicGroupsResults(IList<int> analysisKeys)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Model.TaxonList> GetTaxonListsForUser(Model.UserProfile user)
        {

            using (var db = new DiversityMobile.DiversityMobile())
            {
                return db.Query<TaxonList>("FROM [dbo].[TaxonListsForUser](@0) AS [TaxonList]", user.LoginName).ToList();
            }
        }

        public IEnumerable<Term> GetStandardVocabulary()
        {

            IEnumerable<Term> linqTerms;
            using (var ctx = new DiversityCollectionFunctionsDataContext())
            {
                var taxonomicGroups = from g in ctx.DiversityMobile_TaxonomicGroups()
                                      select new Term()
                                      {
                                          Source = TermList.TaxonomicGroups, //TODO
                                          Code = g.Code,
                                          DisplayText = g.DisplayText
                                      };

                var unitRelationTypes = from t in ctx.DiversityMobile_UnitRelationTypes()
                                        select new Term()
                                        {
                                            Source = TermList.RelationshipTypes, //TODO
                                            Code = t.Code,
                                            DisplayText = t.DisplayText
                                        };

                var eventImgTypes = from eit in ctx.DiversityMobile_EventImageTypes()
                                    select new Term()
                                    {
                                        Source = TermList.EventImageTypes,//TODO
                                        Code = eit.Code,
                                        DisplayText = eit.DisplayText
                                    };
                linqTerms = taxonomicGroups.Concat(unitRelationTypes).Concat(eventImgTypes).ToList();
            }
            return linqTerms;

        }

        public IEnumerable<TaxonName> DownloadTaxonList(TaxonList list)
        {
            using (var db = new DiversityMobile.DiversityMobile())
            {
                //TODO Improve SQL Sanitation
                if (list.Table.Contains(';') ||
                    list.Table.Contains('\'') ||
                    list.Table.Contains('"'))
                    return Enumerable.Empty<TaxonName>();  //SQL Injection ?

                var sql = PetaPoco.Sql.Builder
                    .From(String.Format("[{0}] AS [TaxonName]",list.Table))                    
                    .SQL;
               
                
                return db.Query<TaxonName>(sql).ToList();
            }         
        }

        public IEnumerable<string> GetAvailablePropertyLists()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Model.PropertyName> DownloadPropertyList(string list)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Model.Analysis> GetAnalysesForProject(Project p)
        {
            using (var ctx = new DiversityCollectionFunctionsDataContext())
            {
                var analysisProjectList = from apl in ctx.DiversityMobile_AnalysisProjectList(p.ProjectID)
                                          select new Analysis()
                                          {
                                              AnalysisID = apl.AnalysisID,
                                              Description = apl.Description,
                                              DisplayText = apl.DisplayText,
                                              MeasurementUnit = apl.MeasurementUnit
                                          };
                return analysisProjectList.ToList();
            }
        }
        public IEnumerable<Model.AnalysisResult> GetAnalysisResultsForProject(Project p)
        {
            using (var ctx = new DiversityCollectionFunctionsDataContext())
            {
                var analysisResults = from ar in ctx.DiversityMobile_AnalysisResultForProject(p.ProjectID)
                                      select new AnalysisResult()
                                      {
                                          AnalysisID = ar.AnalysisID,
                                          Description = ar.Description,
                                          DisplayText = ar.DisplayText,
                                          Notes = ar.Notes,
                                          Result = ar.AnalysisResult
                                      };
                return analysisResults.ToList();
            }
        }



        public HierarchySection InsertHierarchy(HierarchySection hierarchy)
        {
            var result = new HierarchySection();
            using (var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities())
            {
                var newEventEntity = hierarchy.Event.ToEntity();
                ctx.CollectionEvent.AddObject(newEventEntity);

                ctx.SaveChanges();
                result.Event = newEventEntity.ToModel();
            }
            return result;
        }
    }
}
