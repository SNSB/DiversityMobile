using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService.Model
{
    public class TermExportList
    {
        public List<Term> Terms;

        public TermExportList()
        {
            Terms = new List<Term>();
        }

        public TermExportList(IEnumerable<Term> terms)
        {
            Terms=new List<Term>();
            foreach (Term t in terms)
                Terms.Add(t);
        }

    }
}
