using DiversityORM;
using DiversityPhone.Model;
using DiversityService.Model;
using System;
using System.Collections.Generic;
using System.Linq;


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
            return login.GetConnection(CATALOG_DIVERSITYMOBILE).Query<PropertyList>("FROM [TermsListsForUser](@0) AS [PropertyList]", login.LoginName);
        }

        private static IEnumerable<Property> getProperties(Diversity db)
        {
            return db.Query<Property>("FROM [Property] AS [Property]");
        }

        private static IEnumerable<Qualification> getQualifications(Diversity db)
        {
            return db.Query<Qualification>("FROM [DiversityMobile_IdentificationQualifiers]() AS [Qualification]");
        }

        private static Event getEvent(Diversity db, int DiversityCollectionID)
        {
            Event ev=db.SingleOrDefault<Event>("FROM CollectionEvent WHERE CollectionEventID=@0", DiversityCollectionID);
            IEnumerable<CollectionEventLocalisation> cel_List = getLocalisationForEvent(db, DiversityCollectionID);
            foreach (CollectionEventLocalisation cel in cel_List)
            {
                if (cel.LocalisationSystemID == 4)
                    try
                    {
                        ev.Altitude = double.Parse(cel.Location1);
                    }
                    catch (Exception) { ev.Altitude = null; }
                if (cel.LocalisationSystemID == 8)
                {
                    try
                    {
                        ev.Longitude = double.Parse(cel.Location1);
                        ev.Latitude = double.Parse(cel.Location2);
                    }
                    catch (Exception) { ev.Longitude = null; ev.Latitude = null; }
                }
            }
            return ev;
        }

        private static IEnumerable<CollectionEventLocalisation> getLocalisationForEvent(Diversity db, int DiversityCollectionID)
        {
            return db.Query<CollectionEventLocalisation>("Select LocalisationSystemID, Location1, Location2 FROM CollectionEventLocalisation WHERE CollectionEventID=@0",DiversityCollectionID);
        }

        private static IEnumerable<EventProperty> getCollectionPropertyForEvent(Diversity db, int DiversityCollectionID)
        {
            return db.Query<EventProperty>("Select CollectionEventID, PropertyID, DisplayText,PropertyURI FROM CollectionEventProperty WHERE CollectionEventID=@0", DiversityCollectionID);
        }

        private static IEnumerable<Specimen> getSpecimenForEvent(Diversity db, int DiversityCollectionID)
        {
            return db.Query<Specimen>("Select CollectionSpecimenID,CollectionEventID, DepositorsAccessionNumber FROM CollectionSpecimen WHERE CollectionEventID=@0", DiversityCollectionID);
        }

        private static IdentificationUnitGeoAnalysis getGeoAnalysisForIU(Diversity db, int DiversityCollectionID)
        {
            //Attention: No Geodata
            return db.SingleOrDefault<IdentificationUnitGeoAnalysis>("Select IdentificationUnitID,CollectionSpecimenID,AnalysisDate From IdentificationUnitGeoAnalysis WHERE IdentificationUNitID0=@0", DiversityCollectionID);
        }

        private static IEnumerable<T> loadTablePaged<T>(string table, int page, Diversity db)
        {
            //TODO Improve SQL Sanitation
            if (table.Contains(";") ||
                table.Contains("'") ||
                table.Contains("\""))
                return Enumerable.Empty<T>();  //SQL Injection ?

            var sql = PetaPoco.Sql.Builder
                .From(string.Format("[dbo].[{0}] AS [{1}]", table, typeof(T).Name))
                .SQL;
            return db.Page<T>(page, 1000, sql).Items;
        }       

    }
}