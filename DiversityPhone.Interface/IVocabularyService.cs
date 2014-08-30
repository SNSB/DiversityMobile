using DiversityPhone.Model;
using System.Collections.Generic;

namespace DiversityPhone.Interface
{
    public interface IVocabularyService
    {
        void clearVocabulary();

        void addTerms(IEnumerable<Term> terms);

        IEnumerable<Term> getTerms(TermList source);

        void addAnalyses(IEnumerable<Analysis> analyses);

        IEnumerable<Analysis> getPossibleAnalyses(string taxonomicGroup);

        Analysis getAnalysisByID(int analysisID);

        void addAnalysisResults(IEnumerable<AnalysisResult> results);

        IEnumerable<AnalysisResult> getPossibleAnalysisResults(int AnalysisID);

        void addAnalysisTaxonomicGroups(IEnumerable<AnalysisTaxonomicGroup> groups);

        void addPropertyNames(IEnumerable<PropertyName> properties);

        IEnumerable<PropertyName> getPropertyNames(int propertyID);

        IEnumerable<Property> getAllProperties();

        void addProperties(IEnumerable<Property> props);

        IEnumerable<Qualification> getQualifications();

        void addQualifications(IEnumerable<Qualification> qualis);
    }
}