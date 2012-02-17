using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DiversityService.Model
{
    public class HierarchySection
    {

        //Nur Modelklassen. Abgeleitet Serverklassen stehen im Kommentar
        public Event Event { get; set; }
        //->CollectionEventLocalisation
        public IList<CollectionEventProperty> Properties { get; set; }

        public IList<Specimen> Specimen { get; set; }
        //->Project
        //->Agent

        public IList<IdentificationUnit> IdentificationUnits { get; set; }
        //->Identification
        //->IdentificationUnitGeoAnalysis

        public IList<IdentificationUnitAnalysis> IdentificationUnitAnalyses { get; set; }

    }
}
