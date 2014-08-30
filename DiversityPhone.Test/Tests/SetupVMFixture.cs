namespace DiversityPhone.Test.Tests
{
    using DiversityPhone.Model;
    using DiversityPhone.ViewModels;
    using Xunit;

    [Trait("ViewModels", "Setup")]
    public class SetupVMFixture : DiversityTestBase<SetupVM>
    {
        private const string VALID_USER = "Valid";
        private const string VALID_PASS = "Pass";
        private const string VALID_DB = "DB";

        public SetupVMFixture()
        {
        }

        [Fact]
        public void CannotSaveInitially()
        {
            Assert.False(T.Save.CanExecute(null));
        }

        [Fact]
        public void CannotSaveWithoutSettings()
        {
            GetT();

            Scheduler.AdvanceBy(100);

            Assert.False(T.Save.CanExecute(null));
        }

        [Fact]
        public void PushesValidCredentials()
        {
            // Setup
            var validCredentials = new Settings()
            {
                UserName = VALID_USER,
                Password = VALID_PASS
            };

            var validWithDB = new Settings()
            {
                UserName = VALID_USER,
                Password = VALID_PASS,
                HomeDBName = VALID_DB
            };

            GetT();

            //Execute
            Assert.True(false);
        }
    }
}