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



namespace DiversityService
{
    public partial class DiversityService : IDiversityService
    {
        //const string entities_connstr_template = "metadata=res://*/DiversityCollectionEntities.csdl|res://*/DiversityCollectionEntities.ssdl|res://*/DiversityCollectionEntities.msl;provider=System.Data.SqlClient;provider connection string=\"{0}\"";

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
                using (var db = new DiversityORM.Diversity(login))
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
            return loadTablePaged<Model.TaxonName>(list.Table, page, DiversityMobile(login));                   
        }

        public IEnumerable<Model.Property> GetPropertiesForUser(UserCredentials login)
        {
            var propsForUser = propertyListsForUser(login).ToDictionary(pl => pl.PropertyID);
            
            using (var db = new DiversityORM.Diversity(login))
            {
                return getProperties(db).Where(p => propsForUser.ContainsKey(p.PropertyID)).ToList();
            }            
        }

        public IEnumerable<Model.PropertyName> DownloadPropertyNames(Property p, int page, UserCredentials login)
        {
            var propsForUser = propertyListsForUser(login).ToDictionary(pl => pl.PropertyID);
            PropertyList list;
            if (propsForUser.TryGetValue(p.PropertyID, out list))
            {                
                return loadTablePaged<Model.PropertyName>(list.Table, page, DiversityMobile(login));                
            }
            else
                return Enumerable.Empty<Model.PropertyName>();
        }

       
        #endregion


        public Dictionary<int, int> InsertEventSeries(IList<EventSeries> series, UserCredentials login)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            using (var db = new DiversityORM.Diversity(login))
            {
                foreach (EventSeries es in series)
                {
                    db.Insert(es);
                    result.Add(es.SeriesID, (int)es.DiversityCollectionEventSeriesID);
                    if (!string.IsNullOrEmpty(es.Geography))
                        InsertGeographyIntoSeries((int) es.DiversityCollectionEventSeriesID, es.Geography, login);
                }
            }
            return result;
        }

        public KeyProjection InsertHierarchy(HierarchySection hierarchy, UserCredentials login)
        {
            KeyProjection result = new KeyProjection();
            using (var db = new DiversityORM.Diversity(login))
            {
                #region event
                if (hierarchy.Event != null) //Event is not synced before-Event is always needed for key or related keys have to be saved and adjusted
                {
                    Event ev = hierarchy.Event;
                    db.Insert(ev);
                    result.eventKey.Add(ev.EventID, (int) ev.DiversityCollectionEventID);
                    foreach (CollectionEventProperty cep in hierarchy.Properties)
                    {
                        cep.DiversityCollectionEventID = ev.DiversityCollectionEventID;
                    }
                    foreach (Specimen spec in hierarchy.Specimen)
                    {
                        spec.DiversityCollectionSpecimenID = ev.DiversityCollectionEventID;
                    }
                    var newLocalisations = PetaPocoProjection.ToLocalisations(ev, login);
                    foreach (CollectionEventLocalisation loc in newLocalisations)
                    {
                        db.Insert(loc);
                    }
                    String geoString = null;
                    try
                    {
                        if (ev.Latitude != null && ev.Longitude != null)
                        {
                            geoString = GlobalUtility.GeographySerialzier.SerializeGeography((double)ev.Latitude, (double)ev.Longitude, ev.Altitude);
                            this.InsertGeographyIntoCollectionEventLocalisation(ev.DiversityCollectionEventID, 8, geoString, login);
                            if (ev.Altitude != null)
                                this.InsertGeographyIntoCollectionEventLocalisation(ev.DiversityCollectionEventID, 4, geoString, login);
                        }
                    }
                    catch (Exception e) {
                        String s = e.Message;
                    }
                }

                IList<CollectionEventProperty> cepList = hierarchy.Properties;
                foreach (CollectionEventProperty cep in cepList)
                    db.Insert(cep);

                #endregion

                #region Specimen

                var specList = hierarchy.Specimen;

                foreach (Specimen spec in specList)
                {
                    db.Insert(spec);
                    db.Insert(PetaPocoProjection.ToProject(spec.DiversityCollectionSpecimenID, login.ProjectID));
                    db.Insert(PetaPocoProjection.ToAgent(spec.DiversityCollectionSpecimenID, login));
                    //AdjustKeys

                    foreach (IdentificationUnit iu in hierarchy.IdentificationUnits)
                    {
                        if (iu.SpecimenID == spec.CollectionSpecimenID)
                            iu.DiversityCollectionSpecimenID = spec.DiversityCollectionSpecimenID;
                    }
                    foreach (IdentificationUnitAnalysis iua in hierarchy.IdentificationUnitAnalyses)
                    {
                        if (iua.SpecimenID == spec.CollectionSpecimenID)
                            iua.DiversityCollectionSpecimenID = spec.DiversityCollectionSpecimenID;
                    }
                    result.specimenKeys.Add(spec.CollectionSpecimenID, spec.DiversityCollectionSpecimenID);
                }

                #endregion

                #region IU

                IList<IdentificationUnit> actualLevelIU = new List<IdentificationUnit>();
                IList<IdentificationUnit> nextLevelIU = new List<IdentificationUnit>();
                foreach (IdentificationUnit iu in hierarchy.IdentificationUnits)
                    if (iu.RelatedUnitID == null)
                        actualLevelIU.Add(iu);

                //Start with top Level IU´s
                foreach (IdentificationUnit iu in actualLevelIU)
                {
                    db.Insert(iu);
                    db.Insert(PetaPocoProjection.ToIdentification(iu,login));
                    db.Insert(PetaPocoProjection.ToGeoAnalysis(iu, login));
                    result.iuKeys.Add(iu.UnitID, iu.DiversityCollectionUnitID);
                    //adjust keys, get directly depending iu´s
                    foreach (IdentificationUnit iUnit in hierarchy.IdentificationUnits)
                    {
                        if(iUnit.RelatedUnitID!=null)
                            if (iUnit.RelatedUnitID == iu.UnitID)
                            {
                                iUnit.DiversityCollectionRelatedUnitID = iu.DiversityCollectionUnitID;
                                nextLevelIU.Add(iUnit);
                            }
                    }
                }

                //iterate trough units until no changes have to be made
                while (nextLevelIU.Count > 0)
                {
                    actualLevelIU = nextLevelIU;
                    nextLevelIU = new List<IdentificationUnit>();
                    foreach (IdentificationUnit iu in actualLevelIU)
                    {
                        db.Insert(iu);
                        db.Insert(PetaPocoProjection.ToIdentification(iu, login));
                        db.Insert(PetaPocoProjection.ToGeoAnalysis(iu, login));
                        result.iuKeys.Add(iu.UnitID, iu.DiversityCollectionUnitID);
                        //adjust keys, get directly depending iu´s
                        foreach (IdentificationUnit iUnit in hierarchy.IdentificationUnits)
                        {
                            if (iUnit.RelatedUnitID == iu.UnitID)
                            {
                                iUnit.DiversityCollectionRelatedUnitID = iu.DiversityCollectionRelatedUnitID;
                                nextLevelIU.Add(iu);
                            }
                        }
                    } 
                }

                //Insert Coordinates and ajust Analyses
                foreach (IdentificationUnit iu in hierarchy.IdentificationUnits)
                {
                    String geoString = null;
                    try
                    {
                        if (iu.Latitude != null && iu.Longitude != null && iu.DiversityCollectionUnitID != null)
                        {
                            geoString = GlobalUtility.GeographySerialzier.SerializeGeography((double)iu.Latitude, (double)iu.Longitude, iu.Altitude);
                            this.InsertGeographyIntoIdentifactionUnitGeoAnalysis((int)iu.DiversityCollectionUnitID, geoString, login);
                        }
                    }
                    catch (Exception e)
                    {
                        String s = e.Message;
                    }
                    foreach (IdentificationUnitAnalysis iua in hierarchy.IdentificationUnitAnalyses)
                    {
                        if (iua.IdentificationUnitID == iu.UnitID)
                            iua.DiversityCollectionUnitID = iu.DiversityCollectionUnitID;
                    }
                }
             

                #endregion

                #region IUA

                var analysisList = hierarchy.IdentificationUnitAnalyses;

                foreach (IdentificationUnitAnalysis iua in analysisList)
                    db.Insert(iua);

                #endregion

                return result;
            }
        }



        #region GeoData
        public void InsertGeographyIntoSeries(int seriesID, String geoString, UserCredentials login)
        {
            if (geoString == null)
                return;
            //Adjust GeoData
            using (var db = new DiversityORM.Diversity(login))
            {
                String sql = "Update [dbo].[CollectionEventSeries] Set geography=" + geoString + " Where SeriesID=" + seriesID;
                db.Execute(sql);
            }
        }

        public void InsertGeographyIntoCollectionEventLocalisation(int eventID, int localisationSystemID, String geoString, UserCredentials login)
        {
            if (geoString == null)
                return;
            //Adjust GeoData
            using (var db = new DiversityORM.Diversity(login))
            {
                String sql = "Update [dbo].[CollectionEventLocalisation] Set geography=" + geoString + " Where CollectionEventID=" + eventID + " AND LocalisationSystemID=" + localisationSystemID;
                db.Execute(sql);
            }
        }

        public void InsertGeographyIntoIdentifactionUnitGeoAnalysis(int unitID, String geoString, UserCredentials login)
        {
            if (geoString == null)
                return;
            //Adjust GeoData
            using (var db = new DiversityORM.Diversity(login))
            {
                String sql = "Update [dbo].[IdentificationUnitGeoAnalysis] Set Geography=" + geoString + " Where IdentificationUnitID=" + unitID;
                db.Execute(sql);
            }
        }
        #endregion

        //#region GeoData 
        //public void InsertGeographyIntoSeries(int seriesID, String geoString, UserCredentials login)
        //{
        //    if (geoString == null)
        //        return;
        //    //Adjust GeoData
        //    using (var ctx = new DiversityCollection.DiversityCollection_MonitoringEntities(Diversity.GetConnectionString(login)))
        //    {
        //        String sql = "Update [dbo].[CollectionEventSeries] Set geography=" + geoString + " Where SeriesID=" + seriesID;
        //        ctx.ExecuteStoreCommand(sql);
        //    }
        //}

        //public void InsertGeographyIntoCollectionEventLocalisation(int eventID, int localisationSystemID, String geoString, UserCredentials login)
        //{
        //    if (geoString == null)
        //        return;
        //    //Adjust GeoData
        //    using (var ctx = new DiversityCollection.DiversityCollection_MonitoringEntities(Diversity.GetConnectionString(login)))
        //    {
        //        String sql = "Update [dbo].[CollectionEventSeries] Set geography=" + geoString + " Where CollectionEventID=" + eventID + " AND LocalisationSystemID=" + localisationSystemID;
        //        ctx.ExecuteStoreCommand(sql);
        //    }
        //}

        //public void InsertGeographyIntoIdentifactionUnitGeoAnalysis(int unitID, int localisationSystemID, String geoString, UserCredentials login)
        //{
        //    if (geoString == null)
        //        return;
        //    //Adjust GeoData
        //    using (var ctx = new DiversityCollection.DiversityCollection_MonitoringEntities(Diversity.GetConnectionString(login)))
        //    {
        //        String sql = "Update [dbo].[IdentificationUnitGeoAnalysis] Set Geography=" + geoString + " Where IdentificationUnitID=" + unitID + " AND LocalisationSystemID=" + localisationSystemID;
        //        ctx.ExecuteStoreCommand(sql);
        //    }
        //}
        //#endregion

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
            using (var ctx = new DiversityCollection.DiversityCollection_MonitoringEntities())
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

            using (var ctx = new DiversityCollection.DiversityCollection_MonitoringEntities())
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
            using (var ctx = new DiversityCollection.DiversityCollection_MonitoringEntities())
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

        public DiversityCollection.CollectionEventSeries InsertEventSeriesForAndroidVESES(EventSeries es)
        {

            using (var ctx = new DiversityCollection.DiversityCollection_MonitoringEntities())
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
