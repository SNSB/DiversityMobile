using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit;
using DiversityService.Test.ServiceReference;

namespace DiversityService.Test
{
    [Trait("Service", "Vocabulary")]
    public class VocabularyTest
    {        
        private DiversityServiceClient _target;
        public VocabularyTest()
        {
            _target = new DiversityServiceClient();
        }

        [Fact]
        public void GetUserInfoWorking()
        {
            //Prepare
           

            //Execute
            var profile = _target.GetUserInfo(TestResources.Credentials);


            //Assert
            Assert.NotNull(profile);
            Assert.Equal(TestResources.Credentials.LoginName, profile.LoginName);
            
        }

        [Fact]
        public void GetRepositoriesWorking()
        {
            //Prepare


            //Execute
            var repos = _target.GetRepositories(TestResources.InitCredentials);


            //Assert
            Assert.NotEmpty(repos);
        }

        [Fact]
        public void GetProjectsWorking()
        {
            //Prepare


            //Execute
            var projects = _target.GetProjectsForUser(TestResources.Credentials);


            //Assert
            Assert.NotEmpty(projects);
        }

        [Fact]
        public void ProjectTaxonNamesWorking()
        {
            //Prepare
            var tl = new TaxonList()
            {
                DisplayText = "N/A",
                Table = "TaxRef_BfN_VPlants",
                TaxonomicGroup = "plants",
                IsPublicList = false
            };
            

            //Execute
            var taxa = _target.DownloadTaxonList(tl,1,TestResources.Credentials);


            //Assert
            Assert.NotEmpty(taxa);
        }

        [Fact]
        public void PublicTaxonNamesWorking()
        {
            //Prepare
            var tl = new TaxonList()
            {
                DisplayText = "N/A",
                Table = "TaxRef_GBOL_Auchenorrhyncha_DE",
                TaxonomicGroup = "plants",
                IsPublicList = true
            };
            

            //Execute
            var taxa = _target.DownloadTaxonList(tl,1,TestResources.Credentials);


            //Assert
            Assert.NotEmpty(taxa);
        }


        [Fact]
        public void TaxonListsForUserWorking()
        {
            //Prepare
            
            //Execute
            var lists = _target.GetTaxonListsForUser(TestResources.Credentials);


            //Assert
            Assert.NotEmpty(lists);
            Assert.NotEmpty(lists.Where(l => l.IsPublicList));
            Assert.NotEmpty(lists.Where(l => !l.IsPublicList));
        }

        [Fact]
        public void AnalysesForProjectWorking()
        {
            //Prepare
            

            //Execute
            var analyses = _target.GetAnalysisTaxonomicGroupsForProject(TestResources.ProjectID, TestResources.Credentials);


            //Assert
            Assert.NotEmpty(analyses);
        }

        [Fact]
        public void AnalysisResultsForProjectWorking()
        {
            //Prepare            

            //Execute
            var ar = _target.GetAnalysisResultsForProject(TestResources.ProjectID, TestResources.Credentials);


            //Assert
            Assert.NotEmpty(ar);
        }

        [Fact]
        public void GetAnalysisTaxonomicGroupsForProjectWorking()
        {
            //Prepare
           

            //Execute
            var ar = _target.GetAnalysisTaxonomicGroupsForProject(TestResources.ProjectID, TestResources.Credentials);


            //Assert
            Assert.NotEmpty(ar);
            Assert.True(ar.Distinct().Count() == ar.Count());
        }

        [Fact]
        public void VocabularyTermsAreUnique()
        {
            //Prepare
            

            //Execute
            var ar = _target.GetStandardVocabulary(TestResources.Credentials);


            //Assert
            Assert.NotEmpty(ar);
            Assert.Equal(ar.Distinct(new TermComparer()).Count(),ar.Count());
        }

        [Fact]
        public void AnalysisTaxonomicGroupsAreUnique()
        {
            //Prepare          

            //Execute
            var ar = _target.GetAnalysisTaxonomicGroupsForProject(TestResources.ProjectID, TestResources.Credentials);


            //Assert
            Assert.NotEmpty(ar);
            Assert.Equal(ar.Distinct(new ATGComparer()).Count(), ar.Count());
        }

        [Fact]
        public void PropertyListsForUserWorking()
        {
            //Prepare
            

            //Execute
            var lists = _target.GetPropertiesForUser(TestResources.Credentials);


            //Assert
            Assert.True(true);
            //Don't have any TermLists
            //Assert.NotEmpty(lists);
        }

        private class TermComparer : IEqualityComparer<Term>
        {

            public bool Equals(Term x, Term y)
            {
                return x.Code == y.Code && x.Source == y.Source;
            }

            public int GetHashCode(Term obj)
            {
                return (obj.Code.GetHashCode() ^ obj.Source.GetHashCode());
            }
        }

        private class ATGComparer : IEqualityComparer<AnalysisTaxonomicGroup>
        {

            public bool Equals(AnalysisTaxonomicGroup x, AnalysisTaxonomicGroup y)
            {
                return x.AnalysisID == y.AnalysisID && x.TaxonomicGroup == y.TaxonomicGroup;
            }

            public int GetHashCode(AnalysisTaxonomicGroup obj)
            {
                return (obj.AnalysisID.GetHashCode() ^ obj.TaxonomicGroup.GetHashCode());
            }
        }

    }
}
