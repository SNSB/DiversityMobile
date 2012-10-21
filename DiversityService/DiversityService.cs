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
        

        #region Get

        public IEnumerable<Term> GetStandardVocabulary(UserCredentials login)
        {

            IEnumerable<Term> linqTerms;
            using (var ctx = new DiversityCollectionFunctionsDataContext(Diversity.GetConnectionString(login, Diversity.SERVER_COLLECTION)))
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
                    return db.Query<UserProfile>("FROM [DiversityMobile_UserInfo]() AS [UserProfile]").Single();
                }
            }
            catch
            {
                return null;
            }

        }

        private static readonly UserCredentials TNT_Login = new UserCredentials() { LoginName = "GBOL", Password = "DWB_2012", Repository="DiversityMobile" };

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

        public IEnumerable<Model.PropertyName> DownloadPropertyNames(Property p, int page, UserCredentials login)
        {
            var propsForUser = propertyListsForUser(login).ToDictionary(pl => pl.PropertyID);
            PropertyList list;
            if (propsForUser.TryGetValue(p.PropertyID, out list))
            {                
                return loadTablePaged<Model.PropertyName>(list.Table, page, new Diversity(login, CATALOG_DIVERSITYMOBILE));                
            }
            else
                return Enumerable.Empty<Model.PropertyName>();
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

        public bool InsertMMO(MultimediaObject mmo, UserCredentials login)
        {
            try
            {
                using (var db = new DiversityORM.Diversity(login))
                {
                    switch (mmo.OwnerType)
                    {
                        case "EventSeries":
                            CollectionEventSeriesImage cesi = MultimediaObject.ToSeriesImage(mmo);
                            db.Insert(cesi);
                            break;
                        case "Event":
                            CollectionEventImage cei = MultimediaObject.ToEventImage(mmo);
                            db.Insert(cei);
                            break;
                        case "Specimen":
                            CollectionSpecimenImage csi=MultimediaObject.ToSpecimenImage(mmo,null);
                            db.Insert(csi);
                            break;
                        case "IdentificationUnit":
                            IdentificationUnit iu = db.Single<IdentificationUnit>(mmo.RelatedId);
                            CollectionSpecimenImage ciui = MultimediaObject.ToSpecimenImage(mmo, iu);
                            db.Insert(ciui);
                            break;
                        default:
                            throw new Exception("unknown type");
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public KeyProjection InsertHierarchy(HierarchySection hierarchy, UserCredentials login)
        {
            KeyProjection result = new KeyProjection();
            using (var db = new DiversityORM.Diversity(login))
            {
                #region event
                try
                {
                    if (hierarchy.Event != null) //Event is not synced before-Event is always needed for key or related keys have to be saved and adjusted
                    {
                        Event ev = hierarchy.Event;
                        try
                        {
                            db.Insert(ev);
                            result.eventKey.Add(ev.EventID, (int)ev.DiversityCollectionEventID);
                        }
                        catch (Exception e)
                        {
                            throw new SyncException("Event:" + ev.EventID + " " + e.Message);
                        }

                        foreach (CollectionEventProperty cep in hierarchy.Properties)
                        {
                            cep.DiversityCollectionEventID = ev.DiversityCollectionEventID;
                        }
                        foreach (Specimen spec in hierarchy.Specimen)
                        {
                            spec.DiversityCollectionEventID = ev.DiversityCollectionEventID;
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
                                geoString = GlobalUtility.GeographySerializer.SerializeGeography((double)ev.Latitude, (double)ev.Longitude, ev.Altitude);
                                this.InsertGeographyIntoCollectionEventLocalisation(ev.DiversityCollectionEventID, 8, geoString, login);
                                if (ev.Altitude != null)
                                    this.InsertGeographyIntoCollectionEventLocalisation(ev.DiversityCollectionEventID, 4, geoString, login);
                            }
                        }
                        catch (Exception)
                        {
                            
                        }
                    }

                }
                catch (Exception)
                {
                   
                    if (!(hierarchy.Event != null && hierarchy.Event.DiversityCollectionEventID > Int32.MinValue))
                    {
                        throw new Exception("Unable to sync");
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
                    if (iu.RelatedUnitID == null || iu.DiversityCollectionRelatedUnitID!=null) //IsTopLevel or RelatedUnit is already in DB
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
                                nextLevelIU.Add(iUnit);
                            }
                        }
                    } 
                }

                //Insert Coordinates and adjust Analyses
                foreach (IdentificationUnit iu in hierarchy.IdentificationUnits)
                {
                    String geoString = null;
                    try
                    {
                        if (iu.Latitude != null && iu.Longitude != null)
                        {
                            geoString = GlobalUtility.GeographySerializer.SerializeGeography((double)iu.Latitude, (double)iu.Longitude, iu.Altitude);
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
