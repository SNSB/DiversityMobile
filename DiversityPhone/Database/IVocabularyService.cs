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
        IEnumerable<PropertyName> getPropertyNames(int propertyID);        

        IEnumerable<Property> getAllProperties();        
        void addProperties(IEnumerable<Property> props);

        IEnumerable<Qualification> getQualifications();
        void addQualifications(IEnumerable<Qualification> qualis);
    }
}
