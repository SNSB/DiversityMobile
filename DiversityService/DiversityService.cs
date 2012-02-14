using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiversityService.Model;
using DiversityMobile;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.ServiceModel;


namespace DiversityService
{
    public class DiversityService : IDiversityService
    {
        #region Get
        public IEnumerable<Project> GetProjectsForUser(UserCredentials login)
        {
            using (var db = new DiversityCollection.DiversityCollection())
            {
                return db.Query<Project>("FROM [DiversityCollection_Test].[dbo].[DiversityMobile_ProjectList] () AS [Project]")
                    .Select(p =>
                        {
                            p.DisplayText = p.DisplayText ?? "No Description";
                            return p;
                        })
                    .ToList(); //TODO Use credential DB
            }            
        }

        public IEnumerable<AnalysisResult> GetAnalysisResults(IList<int> analysisKeys)
        {
            throw new NotImplementedException();
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
  
                linqTerms = taxonomicGroups.Concat(unitRelationTypes).Concat(eventImgTypes).Concat(eventImgTypes).ToList();
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


        public IEnumerable<Project> GetAvailableProjects()
        {
            throw new NotImplementedException();
        }

        public UserProfile GetUserInfo(UserCredentials login)
        {           
            using (var db = new DiversityCollection.DiversityCollection())
            {
                return db.Query<UserProfile>("FROM [DiversityMobile_UserInfo]() AS [UserProfile]").Single(); ;
            }

        }
        #endregion

       


        public Dictionary<EventSeries,EventSeries> InsertEventSeries(IList<EventSeries> series)
        {
            Dictionary<EventSeries,EventSeries> result = new Dictionary<EventSeries, EventSeries>();
            using (var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities())
            {
                //Adjus EventSeries
                foreach (EventSeries es in series)
                {
                    var newSeries = es.ToEntity();
                    ctx.CollectionEventSeries.AddObject(newSeries);
                    ctx.SaveChanges();
                    EventSeries newSeriesBack = newSeries.ToModel();
                    result.Add(es,newSeriesBack);
                }
            }
            return result;
            
        }

        public HierarchySection InsertHierarchy(HierarchySection hierarchy)
        {
            var result = new HierarchySection();
            using (var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities())
            {
                //Adjust Events
                var newEventEntity = hierarchy.Event.ToEntity();
                ctx.CollectionEvent.AddObject(newEventEntity);
                //Store Geodata
                double? altitude = result.Event.Altitude;
                double? latitude = result.Event.Latitude;
                double? longitude = result.Event.Longitude;

                //Save Event
                ctx.SaveChanges();
                result.Event = newEventEntity.ToModel(altitude,latitude,longitude); //Annahme SaveChanges aktualisiert mit anschließendem ToModel aktualisiert Primärschlüssel -- Sonst über Guid suchen
                
                
                //Adjust directly from event depending entities
                var newLocalisations = hierarchy.Event.ToLocalisations(hierarchy.Profile);
                foreach (DiversityCollection.CollectionEventLocalisation loc in newLocalisations)
                {
                    ctx.CollectionEventLocalisation.AddObject(loc);
                }

                var newProperties = hierarchy.Properties.ToEntity(result.Event,hierarchy.Profile);
                foreach (DiversityCollection.CollectionEventProperty prop in newProperties)
                {
                    ctx.CollectionEventProperties.AddObject(prop);
                }
               

                var newSpecimen = hierarchy.Specimen.ToEntity(result.Event);
                foreach (DiversityCollection.CollectionSpecimen spec in newSpecimen)
                {
                    ctx.CollectionSpecimen.AddObject(spec);
                }
                ctx.SaveChanges();
                //TODO: Update CollectionEventLocalisation for geography
                result.Specimen = newSpecimen.ToModel();


                //Ab hier TODO:
                //var newAgents = hierarchy.Specimen.ToAgents();
                //foreach(DiversityCollection.CollectionAgent agent in newAgents)
                //{
                //    ctx.CollectionAgents.AddObject(agent);
                //}
                //var newprojects = hierarchy.Specimen.ToProjects();
                //foreach(DiversityCollection.CollectionProject proj in newprojects)
                //{
                //    ctx.CollectionProjects.AddObject(proj);
                //}
                //var newUnits = hierarchy.IdentificationUnits.ToEntity();
                //foreach (DiversityCollection.IdentificationUnit unit in newUnits)
                //{
                //    ctx.IdentificationUnit.AddObject(unit);
                //}
                //var newIdentifications = hierarchy.IdentificationUnits.ToIdentifications();
                //foreach (DiversityCollection.Identification ident in newIdentifications)
                //{
                //    ctx.Identifications.AddObject(ident);
                //}

                //var newGeoAnalyses=hierarchy.IdentificationUnits.ToGeoanalyses();
                //foreach (DiversityCollection.IdentificationUnitGeoAnalysi iuga in newGeoAnalyses)
                //{
                //    ctx.IdentificationUnitGeoAnalysis.AddObject(iuga);
                //}
                //var newAnalyses = hierarchy.Analyses.ToEntity();
                //foreach (DiversityCollection.IdentificationUnitAnalysis iua in newAnalyses)
                //{
                //    ctx.IdentificationUnitAnalysis.AddObject(iua);
                //}
                


            }
            return result; //Wann Updates von abhängigen Objekten?
        }

        #region utility
        public IEnumerable<Repository> GetRepositories(UserCredentials login)
        {
            return new Repository[]
            {
                new Repository()
                { 
                    DisplayName = "Test",
                    Database = "DiversityCollection_Test"
                },
                //new Repository() // In München funktionen noch nicht implementiert
                //{
                //    DisplayName="DiversityCollection",
                //    Database="DiversityCollection",
                //},
                 //new Repository() // In München funktionen noch nicht implementiert
                //{
                //    DisplayName="DiversityCollection Monitoring",
                //    Database="DiversityCollection_Monitoring",
                //},
            };
        }
        #endregion

        #region XML serialization for Android WebService
        private String Term2XMLSerialization(IEnumerable<Term> linqTerms)
        {
            String xmlString = null;
            TermExportList terms = new TermExportList(linqTerms);
            XmlSerializer ser = new XmlSerializer(typeof(TermExportList));
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            ser.Serialize(memoryStream, terms);
            memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
            xmlString = UTF8ByteArrayToString(memoryStream.ToArray());
            return xmlString;
        }
        public String GetXMLStandardVocabulary()
        {
            IEnumerable<Term> linqTerms = GetStandardVocabulary();
            return Term2XMLSerialization(linqTerms);
        }
        private String UTF8ByteArrayToString(Byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            String constructedString = encoding.GetString(characters);
            return (constructedString);
        }
        #endregion

    }
}
