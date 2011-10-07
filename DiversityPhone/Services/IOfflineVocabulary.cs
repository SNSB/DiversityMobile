namespace DiversityPhone.Services
{
    using System.Collections.Generic;
    using DiversityPhone.Model;

    public interface IOfflineVocabulary
    {
        void addTerms(IEnumerable<Term> terms);

        IList<Term> getTerms(int source);

        void addTaxonNames(IEnumerable<TaxonName> taxa);

        IList<TaxonName> getTaxonNames(Term taxonGroup);
    }
}
