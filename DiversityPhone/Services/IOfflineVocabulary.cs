using System.Collections.Generic;
using DiversityPhone.Model;
using Svc = DiversityPhone.DiversityService;

namespace DiversityPhone.Services
{
   

    public interface IOfflineVocabulary
    {
        void addTerms(IEnumerable<Term> terms);
        IList<Term> getTerms(Svc.TermList source);

        void addAnalyses(IEnumerable<Analysis> analyses);
        IList<Analysis> getAllAnalyses();
        IList<Analysis> getPossibleAnalyses(string taxonomicGroup);
        Analysis getAnalysisByID(int analysisID);

        void addAnalysisResults(IEnumerable<AnalysisResult> results);
        IList<AnalysisResult> getPossibleAnalysisResults(int AnalysisID);

        void addAnalysisTaxonomicGroups(IEnumerable<AnalysisTaxonomicGroup> groups);
        //IList<AnalysisTaxonomicGroup> getAnalysisTaxonomicGroups(string taxonomicGroup);

        void addTaxonNames(IEnumerable<TaxonName> taxa, Svc.TaxonList list);
        int getTaxonTableFreeCount();
        IList<TaxonName> getTaxonNames(Term taxGroup);
        IList<TaxonName> getTaxonNames(Term taxGroup, string genus, string species);

        void addPropertyNames(IEnumerable<PropertyName> properties);
        IList<PropertyName> getPropertyNames(Property prop);
        PropertyName getPropertyNameByURI(string uri);
        IList<Property> getAllProperties();
        Property getPropertyByID(int id);
    }
}
