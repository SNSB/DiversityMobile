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



namespace DiversityService
{
    public partial class DiversityService : IDiversityService
    {
        private const string CATALOG_DIVERSITYMOBILE = "DiversityMobile";

        #region Get

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
                                            DisplayText = t.DisplayText,
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
                using (var db = new DiversityORM.Diversity())
                {
                    return db.Query<UserProfile>("FROM [DiversityMobile_UserInfo]() AS [UserProfile]").Single(); ;
                }
            }
            catch
            {
                return null;
            }

        }

        public IEnumerable<Model.TaxonList> GetTaxonListsForUser(UserCredentials login)
        {
            login.Repository = CATALOG_DIVERSITYMOBILE;
            using (var db = new DiversityORM.Diversity(login))
            {
                return taxonListsForUser(login.LoginName,db).ToList();
            }
        }
      
        public IEnumerable<TaxonName> DownloadTaxonList(TaxonList list, int page, UserCredentials login)
        {
            login.Repository = CATALOG_DIVERSITYMOBILE;
            using (var db = new DiversityORM.Diversity(login))
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

        public IEnumerable<Model.PropertyList> GetPropertyListsForUser(UserCredentials login)
        {
            login.Repository = CATALOG_DIVERSITYMOBILE;
            using (var db = new DiversityORM.Diversity(login))
            {
                return propertyListsForUser(login.LoginName, db).ToList();
            }
        }

        public IEnumerable<Model.PropertyName> DownloadPropertyList(PropertyList list, int page, UserCredentials login)
        {
            login.Repository = CATALOG_DIVERSITYMOBILE;
            using (var db = new DiversityORM.Diversity(login))
            {
                //TODO Improve SQL Sanitation
                if (list.Table.Contains(';') ||
                    list.Table.Contains('\'') ||
                    list.Table.Contains('"'))
                    return Enumerable.Empty<PropertyName>();  //SQL Injection ?

                var sql = PetaPoco.Sql.Builder
                    .From(String.Format("[dbo].[{0}] AS [PropertyName]", list.Table))
                    .SQL;
                var res = db.Page<PropertyName>(page, 1000, sql).Items;
                return res;
            }      
        }

       
        #endregion


        public Dictionary<int,int> InsertEventSeries(IList<EventSeries> series, UserCredentials login)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            using (var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities(Diversity.GetConnectionString(login)))
            {
           
                foreach (EventSeries es in series)
                {
                    var newSeries = es.ToEntity();
                    ctx.CollectionEventSeries.AddObject(newSeries);
                    ctx.SaveChanges();
                    int newSeriesKey = (int)newSeries.SeriesID;            
                    result.Add(es.SeriesID,newSeriesKey);
                    if(!string.IsNullOrEmpty(es.Geography))
                        InsertGeographyIntoSeries(newSeriesKey,es.Geography,login);
                }  
            }
          
            return result;            
        }



        public KeyProjection InsertHierarchy(HierarchySection hierarchy, UserCredentials cred)
        {
            KeyProjection result = new KeyProjection(); 
            using (var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities(Diversity.GetConnectionString(cred)))
            {
                //Adjust Event
                DiversityCollection.CollectionEvent newEventEntity = null;
                #region event
                if (hierarchy.Event != null) //Event is not synced before-Event is always needed for key or related keys have to be saved and adjusted
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
                    result.eventKey.Add(hierarchy.Event.EventID, newEventEntity.CollectionEventID);

                    hierarchy.Event.DiversityCollectionEventID = newEventEntity.CollectionEventID; 
                    foreach (CollectionEventProperty cep in hierarchy.Properties)
                        cep.DiversityCollectionEventID = newEventEntity.CollectionEventID;
                    foreach (Specimen spec in hierarchy.Specimen)
                        spec.DiversityCollectionSpecimenID = newEventEntity.CollectionEventID;

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
                        this.InsertGeographyIntoCollectionEventLocalisation(newEventEntity.CollectionEventID, 8, geoString,cred);
                        if (altitude != null)
                            this.InsertGeographyIntoCollectionEventLocalisation(newEventEntity.CollectionEventID, 4, geoString,cred);
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
                            iu.DiversityCollectionSpecimenID = syncPair.Value.CollectionSpecimenID;
                    }
                }
                ctx.SaveChanges();
                #endregion

                #region IU
                IList<IdentificationUnit> nextLevelIU = new List<IdentificationUnit>();
                foreach (IdentificationUnit iu in hierarchy.IdentificationUnits)
                    if (iu.RelatedUnitID == null)
                        nextLevelIU.Add(iu);

                Dictionary<IdentificationUnit, DiversityCollection.IdentificationUnit> nextIU = nextLevelIU.ToEntity();
                nextLevelIU = new List<IdentificationUnit>();

                //Start with top Level IU´s
                foreach (KeyValuePair<IdentificationUnit, DiversityCollection.IdentificationUnit> kvp in nextIU)
                {
                     ctx.IdentificationUnit.AddObject(kvp.Value);
                }
                ctx.SaveChanges(); //Save for new keys

                //adjust keys, get directly depending iu´s
                foreach (KeyValuePair<IdentificationUnit, DiversityCollection.IdentificationUnit> kvp in nextIU)
                {

                    foreach (IdentificationUnit iu in hierarchy.IdentificationUnits)
                    {
                        if (iu.UnitID == kvp.Key.UnitID)
                            iu.DiversityCollectionUnitID = kvp.Value.IdentificationUnitID;
                        if (iu.RelatedUnitID == kvp.Key.UnitID)
                        {
                            iu.DiversityCollectionRelatedUnitID = kvp.Value.IdentificationUnitID;
                            nextLevelIU.Add(iu);
                        }
                    }
                    result.iuKeys.Add(kvp.Key.UnitID, kvp.Value.IdentificationUnitID);
                }

                //iterate trough units until no changes have to be made
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
                        {
                            if (iu.UnitID == kvp.Key.UnitID)
                                iu.DiversityCollectionUnitID = kvp.Value.IdentificationUnitID;
                            if (iu.RelatedUnitID == kvp.Key.UnitID)
                            {
                                iu.DiversityCollectionRelatedUnitID = kvp.Value.IdentificationUnitID;
                                nextLevelIU.Add(iu);
                            }
                        }
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
                    if (iu.Latitude != null && iu.Longitude != null && iu.DiversityCollectionUnitID!=null)
                    {
                        geoString = GlobalUtility.GeographySerialzier.SerializeGeography((int)iu.Latitude, (int)iu.Longitude, iu.Altitude);
                        this.InsertGeographyIntoIdentifactionUnitGeoAnalysis((int) iu.DiversityCollectionUnitID, 8, geoString,cred);
                        if (iu.Altitude != null)
                            this.InsertGeographyIntoIdentifactionUnitGeoAnalysis((int) iu.DiversityCollectionUnitID, 4, geoString, cred);
                    }
                }
                ctx.SaveChanges();

                #endregion

                #region IUA
                var newAnalyses = hierarchy.IdentificationUnitAnalyses.ToEntity(cred);

                //Add specimenkeys to Analysis
                foreach (IdentificationUnit iu in hierarchy.IdentificationUnits)
                {
                    foreach (KeyValuePair<IdentificationUnitAnalysis, DiversityCollection.IdentificationUnitAnalysis> iuaPair in newAnalyses)
                    {
                        if (iu.DiversityCollectionUnitID == iuaPair.Value.IdentificationUnitID)
                            iuaPair.Value.CollectionSpecimenID = (int) iu.DiversityCollectionSpecimenID;
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

        


        #region GeoData 
        public void InsertGeographyIntoSeries(int seriesID, String geoString, UserCredentials login)
        {
            if (geoString == null)
                return;
            //Adjust GeoData
            using (var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities(Diversity.GetConnectionString(login)))
            {
                String sql = "Update [dbo].[CollectionEventSeries] Set geography=" + geoString + " Where SeriesID=" + seriesID;
                ctx.ExecuteStoreCommand(sql);
            }
        }

        public void InsertGeographyIntoCollectionEventLocalisation(int eventID, int localisationSystemID, String geoString, UserCredentials login)
        {
            if (geoString == null)
                return;
            //Adjust GeoData
            using (var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities(Diversity.GetConnectionString(login)))
            {
                String sql = "Update [dbo].[CollectionEventSeries] Set geography=" + geoString + " Where CollectionEventID=" + eventID + " AND LocalisationSystemID=" + localisationSystemID;
                ctx.ExecuteStoreCommand(sql);
            }
        }

        public void InsertGeographyIntoIdentifactionUnitGeoAnalysis(int unitID, int localisationSystemID, String geoString, UserCredentials login)
        {
            if (geoString == null)
                return;
            //Adjust GeoData
            using (var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities(Diversity.GetConnectionString(login)))
            {
                String sql = "Update [dbo].[IdentificationUnitGeoAnalysis] Set Geography=" + geoString + " Where IdentificationUnitID=" + unitID + " AND LocalisationSystemID=" + localisationSystemID;
                ctx.ExecuteStoreCommand(sql);
            }
        }
        #endregion

        #region utility
        public IEnumerable<Repository> GetRepositories(UserCredentials login)
        {
            
            using (var ctx = new Diversity(login))
            {
                try
                {                                        
                    ctx.OpenSharedConnection(); // validate Credentials
                }
                catch (Exception)
                {
                    return Enumerable.Empty<Repository>();
                }
            }          

            return new Repository[]
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

        public int InsertEventSeriesForAndroid(int SeriesID, String Description)
        {
            EventSeries es = new EventSeries();
            es.SeriesID = SeriesID;
            es.Description = Description;
            using (var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities())
            {
                {
                    var newSeries = es.ToEntity();
                    ctx.CollectionEventSeries.AddObject(newSeries);
                    ctx.SaveChanges();
                    return newSeries.SeriesID;
                }

            }
        }

        public int InsertEventSeriesForAndroidVIntES(EventSeries es)
        {

            using (var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities())
            {
                {
                    var newSeries = es.ToEntity();
                    ctx.CollectionEventSeries.AddObject(newSeries);
                    ctx.SaveChanges();
                    return newSeries.SeriesID;
                }

            }
        }


        public int InsertEventSeriesForAndroidVIntES(String xmlSeries)
        {
            DataContractSerializer ds = new DataContractSerializer(typeof(EventSeries));

            EventSeries es = new EventSeries();
            using (var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities())
            {
                {
                    var newSeries = es.ToEntity();
                    ctx.CollectionEventSeries.AddObject(newSeries);
                    ctx.SaveChanges();
                    return newSeries.SeriesID;
                }

            }
        }

        public EventSeries TestSeriesForAndroid()
        {
            EventSeries es = new EventSeries();
            es.Description = "TestSeries";
            return es;
        }

        public DiversityCollection.CollectionEventSery InsertEventSeriesForAndroidVESES(EventSeries es)
        {

            using (var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities())
            {
                {
                    var newSeries = es.ToEntity();
                    ctx.CollectionEventSeries.AddObject(newSeries);
                    ctx.SaveChanges();
                    return newSeries;
                }

            }
        }


        #endregion

    }
}
