namespace DiversityPhone.Test.Tests
{
    using System;
    using DiversityPhone.ViewModels;
    using Ninject;
    using Xunit;

    [Trait("ViewModels", "Setup")]
    public class SetupVMFixture : DiversityTestBase<SetupVM>
    {


        public SetupVMFixture()
        {
            GetT();
        }

        [Fact]
        public void Cannot_save_initially()
        {
            Assert.False(T.Save.CanExecute(null));
        }

        
    }
}
