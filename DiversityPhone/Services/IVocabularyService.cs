using System.Collections.Generic;
using DiversityPhone.Model;
using Svc = DiversityPhone.DiversityService;

namespace DiversityPhone.Services
{
   

    public interface IVocabularyService
    {
        void clearVocabulary();

        void addTerms(IEnumerable<Term> terms);
        IList<Term> getTerms(Svc.TermList source);

        void addAnalyses(IEnumerable<Analysis> analyses);        
        IList<Analysis> getPossibleAnalyses(string taxonomicGroup);
        Analysis getAnalysisByID(int analysisID);

        void addAnalysisResults(IEnumerable<AnalysisResult> results);
        IList<AnalysisResult> getPossibleAnalysisResults(int AnalysisID);

        void addAnalysisTaxonomicGroups(IEnumerable<AnalysisTaxonomicGroup> groups);        

        void addPropertyNames(IEnumerable<PropertyName> properties);
        IList<PropertyName> getPropertyNames(Property prop);
        PropertyName getPropertyNameByURI(string uri);

        IList<Property> getAllProperties();
        Property getPropertyByID(int id);
    }
}
