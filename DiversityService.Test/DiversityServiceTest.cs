using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit;
using DiversityService.Model;

namespace DiversityService.Test
{
    public class DiversityServiceTest
    {
        private IDiversityService _target;
        public DiversityServiceTest()
        {
            _target = new DiversityService();
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
            var taxa = _target.DownloadTaxonList(tl,0);


            //Assert
            Assert.NotEmpty(taxa);
        }

        [Fact]
        public void taxonNamesForUser_should_work()
        {
            //Prepare
            var profile = new UserProfile()
            {
                LoginName = "rollinger",
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
            var project = new Project()
            {
                ProjectID = 1100,
            };

            //Execute
            var analyses = _target.GetAnalysesForProject(project);


            //Assert
            Assert.NotEmpty(analyses);
        }

        [Fact]
        public void AnalysisResultsForProject_should_work()
        {
            //Prepare
            var project = new Project()
            {
                ProjectID = 1100,
            };

            //Execute
            var ar = _target.GetAnalysisResultsForProject(project);


            //Assert
            Assert.NotEmpty(ar);
        }

        [Fact]
        public void GetAnalysisTaxonomicGroupsForProject_should_work()
        {
            //Prepare
            var project = new Project()
            {
                ProjectID = 1100,
            };

            //Execute
            var ar = _target.GetAnalysisTaxonomicGroupsForProject(project);


            //Assert
            Assert.NotEmpty(ar);
            Assert.True(ar.Distinct().Count() == ar.Count());
        }

    }
}
