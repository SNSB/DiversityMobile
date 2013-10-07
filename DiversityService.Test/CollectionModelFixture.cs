using DiversityService.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;
using Xunit.Extensions;

namespace DiversityService.Test
{
    [Trait("ServiceInternal", "Model")]
    public class CollectionModelFixture
    {
        internal class IUATheoryData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                var arr =
                    new[] {
                        new IdentificationUnitAnalysis(),
                        new IdentificationUnitAnalysis() { CollectionAnalysisDate = "Garbage" },
                        new IdentificationUnitAnalysis() { AnalysisDate = DateTime.MinValue.Date },
                        new IdentificationUnitAnalysis() { CollectionAnalysisDate = DateTime.MinValue.ToString("d", new CultureInfo("de-DE")) }
                    };
                foreach (var arg in arr)
                    yield return new[] { arg };
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        [Theory]
        [ClassData(typeof(IUATheoryData))]
        public void IdentificationDealsWithAnyDateString(IdentificationUnitAnalysis iua)
        {
            Assert.Equal(iua.AnalysisDate, DateTime.MinValue.Date);
        }

    }
}
