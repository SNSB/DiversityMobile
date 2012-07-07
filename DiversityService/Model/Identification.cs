using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService.Model
{
    public class Identification
    {
        public int CollectionSpecimenID{get;set;}
        public int IdentificationUnitID{get;set;}
        public int IdentificationSequence{get;set;}
        public byte? IdentificationDay{get;set;}
        public byte? IdentificationMonth{get;set;}
        public byte? IdentificationYear{get;set;}
        public String IdentificationDateCategory{get;set;}
        public String TaxonomicName{get;set;}
        public String NameURI{get;set;}
        public String IdentificationCategory{get;set;}
        public String ResponsibleName{get;set;}
        public String ResponsibleAgentURI{get;set;}

    }
}
