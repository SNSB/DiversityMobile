using DiversityPhone.Model;
using DiversityPhone.ViewModels;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using Xunit;

namespace DiversityPhone.Test
{
    [Trait("ViewModels", "EditIU")]
    public class EditIUVMFixture : DiversityTestBase<EditIUVM>
    {
        private IEnumerable<Term> taxonGroups = new List<Term>()
        {
            new Term(){SourceID = TermList.TaxonomicGroups, Code = "Test"},
            new Term(){SourceID = TermList.TaxonomicGroups, Code = "Test2"},
        };

        private IEnumerable<TaxonName> taxonNames = new List<TaxonName>()
        {
            new TaxonName(){TaxonNameCache = "Cache1",URI = "Uri1"},
            new TaxonName(){TaxonNameCache = "Cache2",URI = "Uri2"},
        };

        public EditIUVMFixture()
        {
            Vocabulary.Setup(x => x.getQualifications())
                .Returns(() => new List<Qualification>() { new Qualification() } as IEnumerable<Qualification>);
            Vocabulary.Setup(x => x.getTerms(TermList.TaxonomicGroups))
                .Returns(() => taxonGroups);

            Taxa.Setup(x => x.getTaxonNames(It.Is<Term>(t => taxonGroups.Contains(t)), It.IsAny<string>()))
                .Returns<Term, string>((t, q) => (string.IsNullOrWhiteSpace(q)) ? Enumerable.Empty<TaxonName>() : taxonNames);

            T.Activate();
        }

        [Fact]
        public void CannotSaveInitially()
        {
            Scheduler.Start();

            Assert.False(T.Save.CanExecute(null));
        }

        [Fact]
        public void LoadsTaxonNames()
        {
            Assert.Empty(T.Identification.Items);

            T.QueryString = "TestQuery";

            Scheduler.AdvanceBy(100000);

            Assert.NotEmpty(T.Identification.Items);
        }

        [Fact]
        public void CanSaveOnlyIfValid()
        {
            T.QueryString = string.Empty; //Invalid working name

            Scheduler.AdvanceBy(1000);

            Assert.False(T.Save.CanExecute(null));

            T.QueryString = "TestQuery";

            Scheduler.AdvanceBy(1000);

            Assert.True(T.Save.CanExecute(null));
        }
    }
}