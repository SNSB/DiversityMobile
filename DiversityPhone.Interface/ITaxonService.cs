using DiversityPhone.Model;
using System.Collections.Generic;

namespace DiversityPhone.Interface
{
    public interface ITaxonService
    {
        /// <summary>
        /// Adds a new List to the database.
        /// After adding it, the object will have been updated with the table id.
        /// </summary>
        /// <param name="newList"></param>
        void addTaxonList(TaxonList newList);

        /// <summary>
        /// Adds new TaxonNames to the database
        /// </summary>
        /// <param name="taxa">List of TaxonNames</param>
        /// <param name="source">Source</param>
        void addTaxonNames(IEnumerable<TaxonName> taxa, TaxonList source);

        /// <summary>
        /// Queries how many Taxon tables are unused
        /// </summary>
        /// <returns>Number of free Taxon tables</returns>
        int getTaxonTableFreeCount();

        /// <summary>
        /// Updates a Taxon List
        /// </summary>
        /// <param name="sel"></param>
        void updateTaxonList(TaxonList list);

        /// <summary>
        /// Gets all defined Taxon Selections
        /// </summary>
        /// <returns></returns>
        IEnumerable<TaxonList> getTaxonLists();

        /// <summary>
        /// Removes a Taxon Selection and empties its Taxon Table
        /// </summary>
        /// <param name="list"></param>
        void deleteTaxonListIfExists(TaxonList list);

        /// <summary>
        /// Retrieves Taxon Names that conform to the query string
        /// </summary>
        /// <param name="taxGroup"></param>
        /// <param name="query">space separated list of keywords</param>
        /// <returns></returns>
        IEnumerable<TaxonName> getTaxonNames(Term taxGroup, string query);

        /// <summary>
        /// Deletes all Taxon list selections and Databases.
        /// </summary>
        void clearTaxonLists();
    }
}