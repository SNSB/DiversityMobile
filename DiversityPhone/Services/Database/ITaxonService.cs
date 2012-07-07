using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiversityPhone.Model;
using Svc = DiversityPhone.DiversityService;

namespace DiversityPhone.Services
{
    public interface ITaxonService
    {
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
        IList<TaxonList> getTaxonSelections();
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

        /// <summary>
        /// Deletes all Taxon list selections and Databases.
        /// </summary>
        void clearTaxonLists();
    }
}
