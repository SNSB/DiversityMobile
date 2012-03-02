using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit;
using DiversityService.Test.ServiceReference;

namespace DiversityService.Test
{
    public class DiversityServiceTest
    {
        private UserCredentials testCredentials = new UserCredentials()
        {
            LoginName = "Rollinger",
            Password = "Rolli#2-AI4@UBT",
            Repository = "DiversityCollection_Test"
        };

        private Project testProject = new Project()
        {
            ProjectID = 1100,            
        };

        private DiversityServiceClient _target;
        public DiversityServiceTest()
        {
            _target = new DiversityServiceClient();
        }

        //[Fact]
        //public void svc_should_insert_single_event_hierarchy()
        //{
        //    //Prepare
        //    var hierarchy = new HierarchySection()
        //    {
        //        Event = new Event()
        //        {
        //            EventID = 0,
        //            SeriesID = null,
        //            LocalityDescription = "TestLocality",
        //            HabitatDescription = "TestHabitat",
        //            CollectionDate = new DateTime(2001,01,02)                                        
        //        }
        //    };

        //    //Execute
        //    var updatedHierarchy = _target.InsertHierarchy(hierarchy);
        //    var updatedEvent = updatedHierarchy.Event;

            
        //    var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities();
        //    var addedEntity = ( from ev in ctx.CollectionEvent
        //                        where ev.CollectionEventID == updatedEvent.EventID
        //                        select ev).FirstOrDefault();

        //    //Cleanup
        //    if (addedEntity != null)
        //    {
        //        ctx.CollectionEvent.DeleteObject(addedEntity);
        //        ctx.SaveChanges();
        //    }


        //    //Assert
        //    Assert.NotNull(addedEntity); 

        //}

        [Fact]
        public void taxonNames_via_PetaPoco_should_work()
        {
            //Prepare
            var tl = new TaxonList()
            {
                DisplayText = "N/A",
                Table = "TaxRef_BfN_VPlants",
                TaxonomicGroup = "plants"
            };
            

            //Execute
            var taxa = _target.DownloadTaxonList(tl,1,testCredentials);


            //Assert
            Assert.NotEmpty(taxa);
        }

        [Fact]
        public void taxonNamesForUser_should_work()
        {
            //Prepare
            var profile = new UserCredentials()
            {
                LoginName = "Rollinger",
                Password = "Rolli#2-AI4@UBT"
            };

            //Execute
            var lists = _target.GetTaxonListsForUser(profile);


            //Assert
            Assert.NotEmpty(lists);
        }

        [Fact]
        public void AnalysesForProject_should_work()
        {
            //Prepare
            

            //Execute
            var analyses = _target.GetAnalysesForProject(testProject, testCredentials);


            //Assert
            Assert.NotEmpty(analyses);
        }

        [Fact]
        public void AnalysisResultsForProject_should_work()
        {
            //Prepare            

            //Execute
            var ar = _target.GetAnalysisResultsForProject(testProject, testCredentials);


            //Assert
            Assert.NotEmpty(ar);
        }

        [Fact]
        public void GetAnalysisTaxonomicGroupsForProject_should_work()
        {
            //Prepare
           

            //Execute
            var ar = _target.GetAnalysisTaxonomicGroupsForProject(testProject, testCredentials);


            //Assert
            Assert.NotEmpty(ar);
            Assert.True(ar.Distinct().Count() == ar.Count());
        }

        [Fact]
        public void Terms_in_Vocabulary_should_be_unique()
        {
            //Prepare
            

            //Execute
            var ar = _target.GetStandardVocabulary();


            //Assert
            Assert.NotEmpty(ar);
            Assert.Equal(ar.Distinct(new TermComparer()).Count(),ar.Count());
        }

        [Fact]
        public void ATGs_should_be_unique()
        {
            //Prepare          

            //Execute
            var ar = _target.GetAnalysisTaxonomicGroupsForProject(testProject, testCredentials);


            //Assert
            Assert.NotEmpty(ar);
            Assert.Equal(ar.Distinct(new ATGComparer()).Count(), ar.Count());
        }

        private class TermComparer : IEqualityComparer<Term>
        {

            public bool Equals(Term x, Term y)
            {
                return x.Code == y.Code && x.Source == y.Source;
            }

            public int GetHashCode(Term obj)
            {
                return (obj.Code.GetHashCode() ^ obj.Source.GetHashCode());
            }
        }

        private class ATGComparer : IEqualityComparer<AnalysisTaxonomicGroup>
        {

            public bool Equals(AnalysisTaxonomicGroup x, AnalysisTaxonomicGroup y)
            {
                return x.AnalysisID == y.AnalysisID && x.TaxonomicGroup == y.TaxonomicGroup;
            }

            public int GetHashCode(AnalysisTaxonomicGroup obj)
            {
                return (obj.AnalysisID.GetHashCode() ^ obj.TaxonomicGroup.GetHashCode());
            }
        }

    }
}
