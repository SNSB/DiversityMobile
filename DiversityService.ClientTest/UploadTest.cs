using DiversityService.Test.ServiceReference;
using System;
using Xunit;

namespace DiversityService.Test
{
    [Trait("Service", "Upload")]
    public class UploadTest
    {


        private DiversityServiceClient _target;
        public UploadTest()
        {
            _target = new DiversityServiceClient();
        }

        [Fact]
        public void EventSeriesWorking()
        {
            //Prepare
            var es = new EventSeries()
            {
                Description = "TestDescription",
                SeriesCode = "TestCode",
                SeriesStart = DateTime.Now.Subtract(TimeSpan.FromDays(1)),
                SeriesEnd = DateTime.Now
            };

            var locs = new[] 
            { 
                new Localization(){ Longitude = 10.0, Altitude = 1.0, Latitude = 30.0},
                new Localization(){ Longitude = 12.0, Altitude = 1.0, Latitude = 33.0},
                new Localization(){ Longitude = 4.0, Altitude = 1.0, Latitude = 3.0},
            };


            //Execute
            var id = _target.InsertEventSeries(es, locs, TestResources.Credentials);


            //Assert           
            //Nothing to assert            
        }


        [Fact]
        public void EventWorking()
        {
            //Prepare
            var ev = new Event()
            {
                Altitude = 0.0,
                Latitude = 30.0,
                Longitude = 31.0,
                CollectionDate = DateTime.Now.Subtract(TimeSpan.FromDays(1)),
                CollectionSeriesID = -1,
                HabitatDescription = "TestHabitat",
                LocalityDescription = "TestLocality"
            };



            //Execute
            var id = _target.InsertEvent(ev, null, TestResources.Credentials);


            //Assert           
            //Nothing to assert            
        }

        [Fact]
        public void SpecimenWorking()
        {
            //Prepare
            var spec = new Specimen()
            {
                AccessionNumber = "TestAccession",
                CollectionEventID = 1,

            };



            //Execute
            var id = _target.InsertSpecimen(spec, TestResources.Credentials);


            //Assert           
            //Nothing to assert            
        }

        [Fact]
        public void IdentificationUnitWorking()
        {
            //Prepare
            var iu = new IdentificationUnit()
            {
                CollectionSpecimenID = 1,
                Altitude = 0.0,
                Latitude = 30.0,
                Longitude = 31.0,
                IdentificationUri = "TestIdentificationURI",
                LastIdentificationCache = "TestCache",
                TaxonomicGroup = "virus",
                AnalysisDate = DateTime.Now,
                CollectionRelatedUnitID = null


            };



            //Execute
            var id = _target.InsertIdentificationUnit(iu, null, TestResources.Credentials);


            //Assert           
            //Nothing to assert            
        }
    }
}
