using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiversityService.Model;

namespace DiversityService
{
    public class DiversityService : IDiversityService
    {
        //public HierarchySection GetSectionForSeries(int seriesID);
        //public HierarchySection GetSectionForEvent(int eventID);
        //public HierarchySection GetSectionForIU(int iuID);

        

        


        //public void InsertSection(HierarchySection section)
        //{
        //    var db = new DiversityCollection_BaseTestEntities();


        //    var parentEvent = (from ev in db.CollectionEvent where ev.CollectionEventID == EventID select ev).FirstOrDefault();
        //    if (parentEvent != null)
        //    {
        //        var spec = new CollectionSpecimen
        //        {
        //            RowGUID = Guid.NewGuid(),
        //            CollectionEvent = parentEvent
        //        };
        //        var agent = new CollectionAgent
        //        {
        //            CollectorsName = "WP7User",
        //            RowGUID = Guid.NewGuid(),
        //            CollectionSpecimen = spec
        //        };
        //        var project = new CollectionProject()
        //        {
        //            ProjectID = 703,
        //            RowGUID = Guid.NewGuid(),
        //            CollectionSpecimen = spec
        //        };
        //        child.CollectionSpecimen = spec;

        //        db.SaveChanges();
        //    }
        //}

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

        public IList<Term> GetStandardVocabulary()
        {
            var ctx = new DiversityCollection.DiversityCollection_BaseTestEntities();            
            return (from taxGrp in ctx.CollTaxonomicGroup_Enum
                select new Term()
                {
                    SourceID = 0, //TODO
                    Code = taxGrp.Code,
                    Description = taxGrp.Description,
                    DisplayText = taxGrp.DisplayText,
                    ParentCode = taxGrp.ParentCode
                }).ToList();          
        }

        public IList<TaxonName> DownloadTaxonList(string list)
        {
            throw new NotImplementedException();
        }
    }
}
