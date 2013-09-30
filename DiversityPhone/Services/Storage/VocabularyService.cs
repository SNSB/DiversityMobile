
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using DiversityPhone.Model;
using ReactiveUI;
using System.Reactive.Disposables;
using DiversityPhone.Interface;

namespace DiversityPhone.Services
{
    public class VocabularyService : IVocabularyService
    {
        CompositeDisposable _inner = new CompositeDisposable();

        public VocabularyService(IMessageBus msngr)
        {
            _inner.Add(msngr.Listen<Term>(MessageContracts.USE)
                        .Subscribe(term => updateLastUsed(term)));
            _inner.Add(msngr.Listen<Analysis>(MessageContracts.USE)
                       .Subscribe(analysis => updateLastUsed(analysis)));
        }


        public void clearVocabulary()
        {
            using (var ctx = new VocabularyDataContext())
            {
                ctx.DeleteDatabase();
            }
        }

        #region Analyses

        public IEnumerable<Analysis> getPossibleAnalyses(string taxonomicGroup)
        {
            //This query can't be (unordered join) and doesn't have to be (very small) cached 
            return queryDataContext(ctx =>
                from an in ctx.Analyses
                join atg in ctx.AnalysisTaxonomicGroups on an.AnalysisID equals atg.AnalysisID
                where atg.TaxonomicGroup == taxonomicGroup
                orderby an.LastUsed descending
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

        public IEnumerable<AnalysisResult> getPossibleAnalysisResults(int analysisID)
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
                catch (Exception)
                {
                    System.Diagnostics.Debugger.Break();
                }
            });
        }

        #region Terms
        public IEnumerable<Term> getTerms(TermList source)
        {
            return queryDataContext(ctx => from t in ctx.Terms
                                           where t.SourceID == source
                                           orderby t.LastUsed descending, t.DisplayText ascending
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
                 catch (Exception)
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
                //e.g. Relationship when ToplevelIU is saved
                return;
            }          

            withDataContext(ctx =>
            {
                var dbTerm = (from t in ctx.Terms
                              where t.Code == term.Code &&
                                    t.SourceID == term.SourceID
                              select t).Single();
                dbTerm.LastUsed = DateTime.Now;
                ctx.SubmitChanges();
            });
        }

        public void updateLastUsed(Analysis analysis)
        {
            if (analysis == null)
            {


#if DEBUG
                throw new ArgumentNullException("analysis");
#else
                            return;
#endif
                //TODO Log
            }

            withDataContext(ctx =>
            {
                var dbTerm = (from a in ctx.Analyses
                              where a.AnalysisID == analysis.AnalysisID
                              select a).Single();
                dbTerm.LastUsed = DateTime.Now;
                ctx.SubmitChanges();
            });
        }


        #endregion



        #region PropertyNames

        public void addPropertyNames(IEnumerable<PropertyName> properties)
        {
            withDataContext(ctx =>
                {
                    try
                    {

                        ctx.PropertyNames.InsertAllOnSubmit(properties);
                        ctx.SubmitChanges();

                    }
                    catch (Exception)
                    {

                        throw;
                    }

                });

        }

        public IEnumerable<PropertyName> getPropertyNames(int propertyID)
        {
            return enumerateQuery(ctx => from name in ctx.PropertyNames
                                         where name.PropertyID == propertyID
                                         orderby name.DisplayText ascending
                                         select name);
        }

        public IEnumerable<Property> getAllProperties()
        {
            return enumerateQuery(ctx => ctx.Properties);
        }

        public void addProperties(IEnumerable<Property> props)
        {
            withDataContext(ctx =>
                {
                    ctx.Properties.InsertAllOnSubmit(props);
                    ctx.SubmitChanges();
                });
        }
        #endregion

        public IEnumerable<Qualification> getQualifications()
        {
            return enumerateQuery(ctx => ctx.Qualifications);
        }

        public void addQualifications(IEnumerable<Qualification> qualis)
        {
            withDataContext(ctx =>
                {
                    ctx.Qualifications.InsertAllOnSubmit(qualis);
                    try
                    {
                        ctx.SubmitChanges();
                    }
                    catch (Exception)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                });
        }


        private void withDataContext(Action<VocabularyDataContext> operation)
        {
            using (var ctx = new VocabularyDataContext())
            {
                operation(ctx);
            }
        }

        private IEnumerable<T> enumerateQuery<T>(Func<VocabularyDataContext, IQueryable<T>> query)
        {
            using (var ctx = new VocabularyDataContext())
            {
                var q = query(ctx);

                foreach (var res in q)
                {
                    yield return res;
                }
            }
        }

        private IList<T> queryDataContext<T>(Func<VocabularyDataContext, IQueryable<T>> operation)
        {
            using (var ctx = new VocabularyDataContext())
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
            public const string VOCABULARYDB_FILE = "VocabularyDB.sdf";
            private static readonly string DB_URI_PROTOCOL = "isostore:";

            private static string GetCurrentProfileDBPath()
            {
                var profilePath = App.Profile.CurrentProfilePath();
                return string.Format("{0}/{1}/{2}", DB_URI_PROTOCOL, profilePath.Trim('/'), VOCABULARYDB_FILE);
            }

            public VocabularyDataContext()
                : base(GetCurrentProfileDBPath())
            {
                if (!this.DatabaseExists())
                    this.CreateDatabase();
            }

#pragma warning disable 0649
            public Table<Analysis> Analyses;
            public Table<AnalysisResult> AnalysisResults;
            public Table<AnalysisTaxonomicGroup> AnalysisTaxonomicGroups;
            public Table<Term> Terms;

            //Alle PropertyNames werden in derselben Tabelle gespeichert, da die Gesamtzahl in Vergleich zu TaxonNames gering ist.
            public Table<PropertyName> PropertyNames;
            public Table<Property> Properties;

            public Table<Qualification> Qualifications;
#pragma warning restore 0649
        }
    }
}
