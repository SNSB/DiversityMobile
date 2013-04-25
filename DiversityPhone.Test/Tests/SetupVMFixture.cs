namespace DiversityPhone.Test.Tests
{
    using System;
    using DiversityPhone.ViewModels;
    using Ninject;
    using Xunit;
    using System.Reactive.Linq;
    using DiversityPhone.Model;

    [Trait("ViewModels", "Setup")]
    public class SetupVMFixture : DiversityTestBase<SetupVM>
    {


        public SetupVMFixture()
        {
            Settings.Setup(x => x.CurrentSettings()).Returns(Observable.Never<AppSettings>());
            GetT();
        }

        [Fact]
        public void CannotSaveInitially()
        {
            Assert.False(T.Save.CanExecute(null));
        }

        
    }
}
