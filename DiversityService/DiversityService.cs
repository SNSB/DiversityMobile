using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiversityService.Model;
using Diversity;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.ServiceModel;


namespace DiversityService
{
    public class DiversityService : IDiversityService
    {
        private const string CATALOG_DIVERSITYMOBILE = "DiversityMobile";

        #region Get
        public IEnumerable<Project> GetProjectsForUser(UserCredentials login)
        {
            try
            {
                using (var db = new Diversity.Diversity(login))
                {
                    return db.Query<Project>("FROM [dbo].[DiversityMobile_ProjectList] () AS [Project]")
                        .Select(p =>
                            {
                                p.DisplayText = p.DisplayText ?? "No Description";
                                return p;
                            })
                        .ToList(); //TODO Use credential DB
                }
            }
            catch
            {
                return Enumerable.Empty<Project>();
            }
        }

        public IEnumerable<AnalysisResult> GetAnalysisResults(IList<int> analysisKeys)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AnalysisTaxonomicGroup> GetAnalysisTaxonomicGroupsForProject(Project p)
        {
            using (var db = new Diversity.Diversity())
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

        public IEnumerable<Model.TaxonList> GetTaxonListsForUser(UserCredentials login)
        {
            login.Repository = CATALOG_DIVERSITYMOBILE;
            using (var db = new Diversity.Diversity(login))
            {
                return db.Query<TaxonList>("FROM [TaxonListsForUser](@0) AS [TaxonList]", login.LoginName).ToList();
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

        public IEnumerable<TaxonName> DownloadTaxonList(TaxonList list, int page, UserCredentials login)
        {
            login.Repository = CATALOG_DIVERSITYMOBILE;
            using (var db = new Diversity.Diversity(login))
            {
                //TODO Improve SQL Sanitation
                if (list.Table.Contains(';') ||
                    list.Table.Contains('\'') ||
                    list.Table.Contains('"'))
                    return Enumerable.Empty<TaxonName>();  //SQL Injection ?

                var sql = PetaPoco.Sql.Builder
                    .From(String.Format("[dbo].[{0}] AS [TaxonName]",list.Table))                    
                    .SQL;


                var res = db.Page<TaxonName>(page, 1000, sql).Items;
                return res;
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
            using (var db = new Diversity.Diversity())
            {
                return db.Query<Analysis>("FROM [DiversityMobile_AnalysisProjectList](@0) AS [Analysis]", p.ProjectID).ToList();
            }
        }
        public IEnumerable<Model.AnalysisResult> GetAnalysisResultsForProject(Project p)
        {
            using (var db = new Diversity.Diversity())
            {
                return db.Query<AnalysisResult>("FROM [DiversityMobile_AnalysisResultForProject](@0) AS [AnalysisResult]", p.ProjectID).ToList();
                                      
                
            }
        }     

        public UserProfile GetUserInfo(UserCredentials login)
        {
            try
            {
                using (var db = new Diversity.Diversity())
                {
                    return db.Query<UserProfile>("FROM [DiversityMobile_UserInfo]() AS [UserProfile]").Single(); ;
                }
            }
            catch
            {
                return null;
            }

        }
        #endregion


        public Dictionary<int,int> InsertEventSeries(IList<EventSeries> series)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            using (var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities())
            {
                //Adjust EventSeries
                foreach (EventSeries es in series)
                {
                    var newSeries = es.ToEntity();
                    ctx.CollectionEventSeries.AddObject(newSeries);
                    ctx.SaveChanges();                
                    
                    
                    result.Add(es.SeriesID,newSeries.SeriesID);
                    if(!string.IsNullOrEmpty(es.Geography))
                        InsertGeographyIntoSeries(es.SeriesID,es.Geography);
                }
                
            }
          
            return result;
            
        }



        public KeyProjection InsertHierarchy(HierarchySection hierarchy, UserCredentials cred)
        {
            KeyProjection result = new KeyProjection();
            using (var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities())
            {
                //Adjust Event
                DiversityCollection.CollectionEvent newEventEntity = null;
                #region event
                if (hierarchy.Event != null) //Event is not synced before
                {
                    newEventEntity = hierarchy.Event.ToEntity();
                    ctx.CollectionEvent.AddObject(newEventEntity);
                    //Store Geodata
                    double? altitude = hierarchy.Event.Altitude;
                    double? latitude = hierarchy.Event.Latitude;
                    double? longitude = hierarchy.Event.Longitude;

                    //Save Event
                    ctx.SaveChanges();
                    //update keys for Event
                    result.eventKey = new KeyValuePair<int?, int?>(hierarchy.Event.EventID, newEventEntity.CollectionEventID);
                    hierarchy.Event.EventID = newEventEntity.CollectionEventID;
                    foreach (CollectionEventProperty cep in hierarchy.Properties)
                        cep.EventID = newEventEntity.CollectionEventID;
                    foreach (Specimen spec in hierarchy.Specimen)
                        spec.CollectionEventID = newEventEntity.CollectionEventID;


                    //Adjust directly from event depending entities with new key
                    var newLocalisations = hierarchy.Event.ToLocalisations(cred, newEventEntity.CollectionEventID);
                    foreach (DiversityCollection.CollectionEventLocalisation loc in newLocalisations)
                    {
                        ctx.CollectionEventLocalisation.AddObject(loc);
                    }
                    ctx.SaveChanges();
                    String geoString = null;
                    if (latitude != null && longitude != null)
                    {
                        geoString = GlobalUtility.GeographySerialzier.SerializeGeography((int)latitude, (int)longitude, altitude);
                        this.InsertGeographyIntoCollectionEventLocalisation(newEventEntity.CollectionEventID, 8, geoString);
                        if (altitude != null)
                            this.InsertGeographyIntoCollectionEventLocalisation(newEventEntity.CollectionEventID, 4, geoString);
                    }
                }

                var newProperties = hierarchy.Properties.ToEntity(cred);
                foreach (DiversityCollection.CollectionEventProperty prop in newProperties)
                {
                    ctx.CollectionEventProperties.AddObject(prop);
                }
                ctx.SaveChanges();
                #endregion

                #region specimen
                //Specimen
                var newSpecimen = hierarchy.Specimen.ToEntity();

                foreach (KeyValuePair<Specimen, DiversityCollection.CollectionSpecimen> syncPair in newSpecimen)
                {
                    ctx.CollectionSpecimen.AddObject(syncPair.Value);
                }
                ctx.SaveChanges(); //sets keys

                //Sync directly depending Tables

                foreach (KeyValuePair<Specimen, DiversityCollection.CollectionSpecimen> syncPair in newSpecimen)
                {
                    //Sync Projects
                    ctx.CollectionProjects.AddObject(ModelProjection.ToProject(syncPair.Value.CollectionSpecimenID, cred.ProjectID));
                    //Sync Agents
                    ctx.CollectionAgents.AddObject(ModelProjection.ToAgent(syncPair.Value.CollectionSpecimenID, cred));
                    //adjust keys
                    result.specimenKeys.Add(syncPair.Key.CollectionSpecimenID, syncPair.Value.CollectionSpecimenID);
                    foreach (IdentificationUnit iu in hierarchy.IdentificationUnits)
                    {
                        if (iu.SpecimenID == syncPair.Key.CollectionSpecimenID)
                            iu.SpecimenID = syncPair.Value.CollectionSpecimenID;
                    }
                }
                ctx.SaveChanges();
                #endregion

                #region IU
                IList<IdentificationUnit> nextLevelIU = new List<IdentificationUnit>();
                Dictionary<IdentificationUnit, DiversityCollection.IdentificationUnit> synchronizedIU = new Dictionary<IdentificationUnit, DiversityCollection.IdentificationUnit>();
                foreach (IdentificationUnit iu in hierarchy.IdentificationUnits)
                    if (iu.RelatedUnitID == null)
                        nextLevelIU.Add(iu);

                Dictionary<IdentificationUnit, DiversityCollection.IdentificationUnit> nextIU = nextLevelIU.ToEntity();
                nextLevelIU = new List<IdentificationUnit>();

                //Start with top Level IU´s
                foreach (KeyValuePair<IdentificationUnit, DiversityCollection.IdentificationUnit> kvp in nextIU)
                {
                    if (kvp.Key.RelatedUnitID == null)
                    {
                        ctx.IdentificationUnit.AddObject(kvp.Value);
                    }
                }
                ctx.SaveChanges(); //Save for new keys

                //adjust keys, get directly depending iu´s
                foreach (KeyValuePair<IdentificationUnit, DiversityCollection.IdentificationUnit> kvp in nextIU)
                {
                    foreach (IdentificationUnit iu in hierarchy.IdentificationUnits)
                        if (iu.RelatedUnitID == kvp.Key.UnitID)
                        {
                            iu.RelatedUnitID = kvp.Value.IdentificationUnitID;
                            nextLevelIU.Add(iu);
                        }
                    synchronizedIU.Add(kvp.Key, kvp.Value);
                    result.iuKeys.Add(kvp.Key.UnitID, kvp.Value.IdentificationUnitID);
                }

                //iterate trhoug units until no changes have to be made
                while (nextLevelIU.Count > 0)
                {
                    nextIU = nextLevelIU.ToEntity();
                    nextLevelIU = new List<IdentificationUnit>();
                    foreach (KeyValuePair<IdentificationUnit, DiversityCollection.IdentificationUnit> kvp in nextIU)
                    {
                        ctx.IdentificationUnit.AddObject(kvp.Value);
                    }

                    ctx.SaveChanges(); //get new keys
                    //adjust keys,get iu´s for next round
                    foreach (KeyValuePair<IdentificationUnit, DiversityCollection.IdentificationUnit> kvp in nextIU)
                    {
                        foreach (IdentificationUnit iu in hierarchy.IdentificationUnits)
                            if (iu.RelatedUnitID == kvp.Key.UnitID)
                            {
                                iu.RelatedUnitID = kvp.Value.IdentificationUnitID;
                                nextLevelIU.Add(kvp.Key);
                            }
                        synchronizedIU.Add(kvp.Key, kvp.Value);
                        result.iuKeys.Add(kvp.Key.UnitID, kvp.Value.IdentificationUnitID);
                    }
                }

                var newIdentifications = hierarchy.IdentificationUnits.ToIdentifications(cred);
                foreach (DiversityCollection.Identification ident in newIdentifications)
                {
                    ctx.Identifications.AddObject(ident);
                }
                var newGeoAnalyses = hierarchy.IdentificationUnits.ToGeoAnalyses(cred);
                foreach (DiversityCollection.IdentificationUnitGeoAnalysi iuga in newGeoAnalyses)
                {
                    ctx.IdentificationUnitGeoAnalysis.AddObject(iuga);
                }
                ctx.SaveChanges();
                //Insert Coordinates
                foreach (IdentificationUnit iu in hierarchy.IdentificationUnits)
                {
                    String geoString = null;
                    if (iu.Latitude != null && iu.Longitude != null)
                    {
                        geoString = GlobalUtility.GeographySerialzier.SerializeGeography((int)iu.Latitude, (int)iu.Longitude, iu.Altitude);
                        this.InsertGeographyIntoIdentifactionUnitGeoAnalysis(newEventEntity.CollectionEventID, 8, geoString);
                        if (iu.Altitude != null)
                            this.InsertGeographyIntoIdentifactionUnitGeoAnalysis(newEventEntity.CollectionEventID, 4, geoString);
                    }
                }
                ctx.SaveChanges();
                foreach (KeyValuePair<IdentificationUnit, DiversityCollection.IdentificationUnit> syncPair in synchronizedIU)
                {
                    foreach (IdentificationUnitAnalysis iua in hierarchy.IdentificationUnitAnalyses)
                    {
                        if (iua.IdentificationUnitID == syncPair.Key.UnitID)
                            iua.IdentificationUnitID = syncPair.Value.IdentificationUnitID;
                    }
                }
                #endregion

                #region IUA
                var newAnalyses = hierarchy.IdentificationUnitAnalyses.ToEntity(cred);

                //Add specimenkeys to Analysis
                foreach (IdentificationUnit iu in hierarchy.IdentificationUnits)
                {
                    foreach (KeyValuePair<IdentificationUnitAnalysis, DiversityCollection.IdentificationUnitAnalysis> iuaPair in newAnalyses)
                    {
                        if (iu.UnitID == iuaPair.Value.IdentificationUnitID)
                            iuaPair.Value.SpecimenPartID = iu.SpecimenID;
                    }
                }
                foreach (KeyValuePair<IdentificationUnitAnalysis, DiversityCollection.IdentificationUnitAnalysis> syncPair in newAnalyses)
                {
                    ctx.IdentificationUnitAnalysis.AddObject(syncPair.Value);
                }
                ctx.SaveChanges();
                #endregion
            }
            return result;
        }

        public HierarchySection CreateNewHierarchy()
        {
            return new HierarchySection();
        }


        #region GeoData
        public void InsertGeographyIntoSeries(int seriesID, String geoString)
        {
            if (geoString == null)
                return;
            //Adjust GeoData
            using (var db = new Diversity.Diversity())
            {
                String sql = "Update [dbo].[CollectionEventSeries] Set geography=" + geoString + " Where SeriesID=" + seriesID;
                db.Execute(sql);
            }
        }

        public void InsertGeographyIntoCollectionEventLocalisation(int eventID, int localisationSystemID, String geoString)
        {
            if (geoString == null)
                return;
            //Adjust GeoData
            using (var db = new Diversity.Diversity())
            {
                String sql = "Update [dbo].[CollectionEventSeries] Set geography=" + geoString + " Where CollectionEventID=" + eventID + " AND LocalisationSystemID=" + localisationSystemID;
                db.Execute(sql);
            }
        }

        public void InsertGeographyIntoIdentifactionUnitGeoAnalysis(int unitID, int localisationSystemID, String geoString)
        {
            if (geoString == null)
                return;
            //Adjust GeoData
            using (var db = new Diversity.Diversity())
            {
                String sql = "Update [dbo].[IdentificationUnitGeoAnalysis] Set Geography=" + geoString + " Where IdentificationUnitID=" + unitID + " AND LocalisationSystemID=" + localisationSystemID;
                db.Execute(sql);
            }
        }
        #endregion

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
