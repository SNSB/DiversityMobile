using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiversityPhone.Model;

namespace DiversityPhone.Services
{
    public interface IOfflineVocabulary
    {
        void addTerms(IEnumerable<Term> terms);
        IList<Term> getTerms(int source);

        void addTaxonNames(IEnumerable<TaxonName> taxa);
        IList<TaxonName> getTaxonNames(Term taxonGroup);
    }
}
