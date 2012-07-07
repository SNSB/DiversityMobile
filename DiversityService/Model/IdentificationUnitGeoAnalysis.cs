using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;

namespace DiversityService.Model
{
    public class IdentificationUnitGeoAnalysis
    {
        public DateTime AnalysisDate{get;set;}
        public int IdentificationUnitID{get;set;}
        public int CollectionSpecimenID{get;set;}
        public String ResponsibleName{get;set;}
        public String ResponsibleAgentURI{get;set;}

    }
}
