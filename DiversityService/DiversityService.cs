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
            //using (var db = new DiversityCollection.DiversityCollection())
            //{
            //    return db.Query<Project>
            //}

            return Enumerable.Empty<Project>();
        }

        public IEnumerable<AnalysisResult> GetAnalysisResults(IList<int> analysisKeys)
        {
            return null; //TODO
        }

        public IEnumerable<AnalysisTaxonomicGroup> GetAnalysisTaxonomicGroupsForProject(Project p)
        {
            using (var db = new DiversityCollection.DiversityCollection())
            {
                var flattenQueue = new Queue<AnalysisTaxonomicGroup>(db.Query<AnalysisTaxonomicGroup>("FROM [DiversityMobile_AnalysisTaxonomicGroupsForProject](@0) AS [AnalysisTaxonomicGroup]", p.ProjectID));                    
                var flattened = new List<AnalysisTaxonomicGroup>(flattenQueue.Count);
                var analyses = GetAnalysesForProject(p);

                while(flattenQueue.Count > 0)
                {
                    var atg = flattenQueue.Dequeue();
                    flattened.Add(atg);
                    var childANs = from an in analyses
                                   where an.AnalysisParentID == atg.AnalysisID
                                   select an;
                    foreach (var an in childANs)
                    {
                        flattenQueue.Enqueue(new AnalysisTaxonomicGroup() { AnalysisID = an.AnalysisID, TaxonomicGroup = atg.TaxonomicGroup });
                    }
                }
                return flattened;
            }
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

        public IEnumerable<TaxonName> DownloadTaxonList(TaxonList list, int page)
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


                return db.Page<TaxonName>(page, 1000, sql).Items;               
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
            using (var db = new DiversityCollection.DiversityCollection())
            {
                return db.Query<Analysis>("FROM [DiversityMobile_AnalysisProjectList](@0) AS [Analysis]", p.ProjectID).ToList();
            }
        }
        public IEnumerable<Model.AnalysisResult> GetAnalysisResultsForProject(Project p)
        {
            using (var db = new DiversityCollection.DiversityCollection())
            {
                return db.Query<AnalysisResult>("FROM [DiversityMobile_AnalysisResultForProject](@0) AS [AnalysisResult]", p.ProjectID).ToList();
                                      
                
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


        public IEnumerable<Project> GetAvailableProjects()
        {
            throw new NotImplementedException();
        }
    }
}
