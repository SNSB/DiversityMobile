using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace DiversityService.Test
{
    [Trait("Service", "Download")]
    public class DownloadTest
    {
        readonly ServiceReference.DiversityServiceClient Target;

        public DownloadTest()
        {
            Target = new ServiceReference.DiversityServiceClient();
        }

        [Fact]
        public void SeriesShouldRetrieveCorrectInfo()
        {
            var series = Target.EventSeriesByID(TestResources.SeriesID, TestResources.Credentials);

            Assert.Equal(TestResources.SeriesID, series.CollectionEventSeriesID);
            Assert.Equal("TestDescription", series.Description);
            Assert.Equal("TestCode", series.SeriesCode);
            Assert.Equal(DateTime.Parse("2013-03-22 15:19:09.410"), series.SeriesStart);
            Assert.Equal(DateTime.Parse("2013-03-23 15:19:09.413"), series.SeriesEnd);
        }

        [Fact(Skip="Not supported on the Server yet")]        
        public void EventShouldRetrieveCorrectInfo()
        {
            var ev = Target.EventsByLocality("TestLocality", TestResources.Credentials).Single();

            Assert.Equal(1, ev.CollectionEventID);
            Assert.Equal(TestResources.SeriesID, ev.CollectionSeriesID);
            Assert.Equal(DateTime.Parse("2013-03-22 00:00:00.000"), ev.CollectionDate);
            Assert.Equal("TestLocality", ev.LocalityDescription);
            Assert.Equal("TestHabitat", ev.HabitatDescription);
        }

        [Fact]
        public void SpecimenRetrievesCorrectInfo()
        {
            var spec = Target.SpecimenForEvent(TestResources.EventID, TestResources.Credentials).Where(s => s.CollectionSpecimenID == TestResources.SpecimenID).Single();

            Assert.Equal(TestResources.EventID, spec.CollectionEventID);
            Assert.Equal("TestAccession", spec.AccessionNumber);
        }

        [Fact(Skip="Server Support missing")]
        public void IdentificationUnitRetrievesCorrectInfo()
        {
            var iu = Target.UnitsForSpecimen(TestResources.SpecimenID, TestResources.Credentials).Where(u => u.CollectionUnitID == TestResources.UnitID).Single();

            Assert.Equal(TestResources.SpecimenID, iu.CollectionSpecimenID);
            Assert.Equal("TestCache", iu.LastIdentificationCache);
            Assert.Equal("TestFamily", iu.FamilyCache);
            Assert.Equal("TestOrder", iu.OrderCache);
            Assert.Equal("plant", iu.TaxonomicGroup);
        }

    }
}
