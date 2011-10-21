using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiversityService.Model;

namespace DiversityService
{
    public class DiversityService : IDiversityService
    {        
        public IList<Model.EventSeries> GetSeriesByDescription(string description)
        {
            throw new NotImplementedException();
        }

       

        public IList<Model.EventSeries> AllEventSeries()
        {
            throw new NotImplementedException();
        }

        public IList<Model.Project> GetProjectsForUser(Model.UserProfile user)
        {
            throw new NotImplementedException();
        }


        public IList<Model.TermList> GetTaxonListsForUser(Model.UserProfile user)
        {
            throw new NotImplementedException();
        }             

        public IEnumerable<Term> GetStandardVocabulary()
        {
            using (var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities())
            {
                var taxonGroups =
                     from taxGrp in ctx.CollTaxonomicGroup_Enum
                     select new Term()
                     {
                         SourceID = 0, //TODO
                         Code = taxGrp.Code,
                         Description = taxGrp.Description,
                         DisplayText = taxGrp.DisplayText,
                         ParentCode = taxGrp.ParentCode
                     };
                var relationTypes =
                    from relType in ctx.CollUnitRelationType_Enum
                    select new Term()
                    {
                        SourceID = 1,
                        Code = relType.Code,
                        Description = relType.Description,
                        DisplayText = relType.DisplayText,
                        ParentCode = relType.ParentCode
                    };

                return Enumerable.Concat(taxonGroups, relationTypes).ToList(); //Iterate over Enumerables so the context can be disposed safely.
            }
           
        }

        public IEnumerable<TaxonName> DownloadTaxonList(string list)
        {
            using (var ctx = new DiversityMobileEntities())
            {
                return (from tn in ctx.TaxRef_BfN_VPlants
                       select new TaxonName()
                       {
                           URI = tn.NameURI,
                           TaxonNameCache = tn.TaxonNameCache,
                           TaxonNameSinAuth = tn.TaxonNameSinAuthors,
                           GenusOrSupragenic = tn.GenusOrSupragenericName,
                           SpeciesEpithet = tn.SpeciesEpithet,
                           InfraspecificEpithet = tn.InfraspecificEpithet
                       }).ToList();
            }
        }

        public IEnumerable<Model.PropertyName> DownloadPropertyList(string list)
        {
            throw new NotImplementedException();
        }

    }
}
