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

        /// <summary>
        /// Adds new TaxonNames to the database
        /// </summary>        
        /// <param name="taxa">List of TaxonNames</param>
        /// <param name="source">Source</param>
        void addTaxonNames(IEnumerable<TaxonName> taxa, Svc.TaxonList source);
        /// <summary>
        /// Queries how many Taxon tables are unused
        /// </summary>
        /// <returns>Number of free Taxon tables</returns>
        int getTaxonTableFreeCount();
        /// <summary>
        /// Updates a Taxon Selection
        /// </summary>
        /// <param name="sel"></param>
        void selectTaxonList(Svc.TaxonList list);
        /// <summary>
        /// Gets all defined Taxon Selections
        /// </summary>
        /// <returns></returns>
        IList<TaxonSelection> getTaxonSelections();
        /// <summary>
        /// Removes a Taxon Selection and empties its Taxon Table
        /// </summary>
        /// <param name="selection"></param>
        void deleteTaxonList(Svc.TaxonList list);
        
        /// <summary>
        /// Retrieves Taxon Names that conform to the query string
        /// </summary>
        /// <param name="taxGroup"></param>
        /// <param name="query">space separated list of keywords</param>
        /// <returns></returns>
        IList<TaxonName> getTaxonNames(Term taxGroup, string query);

        void addPropertyNames(IEnumerable<PropertyName> properties);
        IList<PropertyName> getPropertyNames(Property prop);
        PropertyName getPropertyNameByURI(string uri);
        IList<Property> getAllProperties();
        Property getPropertyByID(int id);
    }
}
