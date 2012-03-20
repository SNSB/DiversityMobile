
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using DiversityPhone.Model;
using Svc = DiversityPhone.DiversityService;
using ReactiveUI;
using DiversityPhone.Messages;
using System.Reactive.Disposables;

namespace DiversityPhone.Services
{
    public class VocabularyService : IVocabularyService
    {
        CompositeDisposable _inner = new CompositeDisposable();

        public VocabularyService(IMessageBus msngr)
        {
            _inner.Add(msngr.Listen<Term>(MessageContracts.USE)
                        .Subscribe(term => updateLastUsed(term)));
        }


         public void clearVocabulary()
        {
            withDataContext(ctx => { ctx.DeleteDatabase(); ctx.CreateDatabase(); });
        }

        #region Analyses     

        public IList<Analysis> getPossibleAnalyses(string taxonomicGroup)
        {
            //This query can't be (unordered join) and doesn't have to be (very small) cached 
            return queryDataContext(ctx =>
                from an in ctx.Analyses
                join atg in ctx.AnalysisTaxonomicGroups on an.AnalysisID equals atg.AnalysisID
                where atg.TaxonomicGroup == taxonomicGroup
                select an);
        }

        public Analysis getAnalysisByID(int id)
        {
            return singleDataContext(ctx => from an in ctx.Analyses
                                         where an.AnalysisID == id
                                         select an);
        }

        public void addAnalyses(IEnumerable<Analysis> analyses)
        {
            withDataContext(ctx =>
            {
                ctx.Analyses.InsertAllOnSubmit(analyses);
                ctx.SubmitChanges();
            });
        }
        #endregion
        #region AnalysisResults

        public IList<AnalysisResult> getPossibleAnalysisResults(int analysisID)
        {
            return queryDataContext(ctx =>
                from ar in ctx.AnalysisResults
                where ar.AnalysisID == analysisID
                select ar
            );
        }
        public void addAnalysisResults(IEnumerable<AnalysisResult> results)
        {
            withDataContext(ctx =>
                {
                    ctx.AnalysisResults.InsertAllOnSubmit(results);
                    ctx.SubmitChanges();
                });
        }
        #endregion
        public void addAnalysisTaxonomicGroups(IEnumerable<AnalysisTaxonomicGroup> groups)
        {
            withDataContext(ctx =>
            {
                ctx.AnalysisTaxonomicGroups.InsertAllOnSubmit(groups);
                try
                {
                    ctx.SubmitChanges();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debugger.Break();
                }
            });
        }

        #region Terms
        public IList<Term> getTerms(Svc.TermList source)
        {
            return queryDataContext(ctx => from t in ctx.Terms
                                        where t.SourceID == source
                                        orderby t.LastUsed descending
                                        select t
                                        );
        }



        public void addTerms(IEnumerable<Term> terms)
        {
            withDataContext(ctx =>
             {

                 ctx.Terms.InsertAllOnSubmit(terms);
                 try
                 {
                     ctx.SubmitChanges();
                 }
                 catch (Exception ex)
                 {
                     System.Diagnostics.Debugger.Break();
                     //TODO Log
                 }

             });            
        }

        public void updateLastUsed(Term term)
        {
            if (term == null)
            {
#if DEBUG
                throw new ArgumentNullException("term");
#else
                return;
#endif
                //TODO Log
            }

            withDataContext(ctx =>
            {
                ctx.Terms.Attach(term);
                term.LastUsed = DateTime.Now;
                ctx.SubmitChanges();
            });
        }

        #endregion

        

        #region PropertyNames

        public void addPropertyNames(IEnumerable<PropertyName> properties)
        {
            withDataContext(ctx =>
                {
                    ctx.PropertyNames.InsertAllOnSubmit(properties);
                    ctx.SubmitChanges();
                });
            
        }

        public IList<PropertyName> getPropertyNames(Property prop)
        {
            return queryDataContext(ctx => from pn in ctx.PropertyNames
                                        where pn.PropertyID == prop.PropertyID
                                        select pn);
        }

        public PropertyName getPropertyNameByURI(string uri)
        {
            return singleDataContext(ctx => from pn in ctx.PropertyNames
                          where pn.PropertyUri == uri
                          select pn);
            
        }

        public IList<Property> getAllProperties()
        {
            return queryDataContext(ctx => ctx.Properties);
        }

        public Property getPropertyByID(int id)
        {
            return singleDataContext(ctx =>
                from p in ctx.Properties
                where p.PropertyID == id
                select p);
        }



        #endregion

        private void withDataContext(Action<VocabularyDataContext> operation)
        {
            using (var ctx = new VocabularyDataContext())
            {
                operation(ctx);
            }
        }

        private IList<T> queryDataContext<T>(Func<VocabularyDataContext, IQueryable<T>> operation)
        {
            using(var ctx = new VocabularyDataContext())
	        {
		        return operation(ctx).ToList();
	        }            
        }

        private T singleDataContext<T>(Func<VocabularyDataContext, IQueryable<T>> operation)
        {
            using (var ctx = new VocabularyDataContext())
            {
                return operation(ctx).SingleOrDefault();
            }
        }

        private class VocabularyDataContext : DataContext
        {
            private static string connStr = "isostore:/vocabularyDB.sdf";

            public VocabularyDataContext()
                : base(connStr)
            {
                if (!this.DatabaseExists())
                    this.CreateDatabase();
            }
            
            public Table<Analysis> Analyses;
            public Table<AnalysisResult> AnalysisResults;
            public Table<AnalysisTaxonomicGroup> AnalysisTaxonomicGroups;
            public Table<Term> Terms;

            //Alle PropertyNames werden in derslben Tabelle gespeichert, da die Gesamtzahl in Vergleich zu TaxonNames gering ist.
            public Table<PropertyName> PropertyNames;
            public Table<Property> Properties;
        }




       
    }
}
