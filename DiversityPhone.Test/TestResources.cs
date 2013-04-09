using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiversityPhone.Test
{
    class TestResources
    {
        public readonly EventSeries EventSeries = new EventSeries() { SeriesID = 0, CollectionSeriesID = -1 };
        public readonly Event Event = new Event() { EventID = 1, SeriesID = -1, CollectionEventID = 0};
        public readonly Specimen Specimen = new Specimen() { SpecimenID = 2, EventID = 0, CollectionSpecimenID = 1 };
        public readonly IdentificationUnit Unit = new IdentificationUnit() { UnitID = 3, SpecimenID = 0, CollectionUnitID = 2, RelatedUnitID = null };
    }
}
