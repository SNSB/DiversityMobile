using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq.Mapping;
using DiversityPhone.Services;

namespace DiversityPhone.Model
{

    public class TaxonSynonymyPair
    {
        public TaxonName Accepted { get; set; }
        public TaxonName Synonym { get; set; }
        public TaxonName Selected { get; set; }

        public TaxonSynonymyPair()
        {
        }

        public TaxonSynonymyPair(TaxonName accepted)
        {
            Accepted = accepted;
            Selected = Accepted;
        }

        public TaxonSynonymyPair(TaxonName accepted,TaxonName synonym) :  this(accepted)
        {
            Synonym = synonym;
        }
    }
}
