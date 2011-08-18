using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiversityService.Model;

namespace DiversityService
{
    public static class TermLists
    {
        public static readonly TermList TaxonomicGroups;

        

        static TermLists()
        {
            int id = 0;
            TaxonomicGroups = new TermList(id, "Taxonomic Groups");
        


        }
    }
}
