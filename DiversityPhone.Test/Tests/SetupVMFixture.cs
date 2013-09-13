namespace DiversityPhone.Test.Tests
{
    using System;
    using DiversityPhone.ViewModels;
    using Ninject;
    using Xunit;
    using System.Reactive.Linq;
    using DiversityPhone.Model;
    using System.Reactive.Subjects;
    using Microsoft.Reactive.Testing;
    using Moq;

    [Trait("ViewModels", "Setup")]
    public class SetupVMFixture : DiversityTestBase<SetupVM>
    {
        const string VALID_USER = "Valid";
        const string VALID_PASS = "Pass";
        const string VALID_DB = "DB";

        public SetupVMFixture()
        {
            RecursiveMock<IValidateLogin>();
            RecursiveMock<ISelectDatabase>();
            RecursiveMock<ISelectProject>();
            RecursiveMock<IGetUserProfile>();
        }

        [Fact]
        public void CannotSaveInitially()
        {
            Assert.False(T.Save.CanExecute(null));
        }

        [Fact]
        public void CannotSaveWithoutSettings()
        {
            var login = RecursiveMock<IValidateLogin>();
            login.Setup(x => x.IsLoginValid).Returns(
                Scheduler.CreateColdObservable(OnNext(0, true))
                );
            RecursiveMock<ISelectDatabase>()
                .Setup(x => x.IsDatabaseSelected)
                .Returns(Scheduler.CreateColdObservable(OnNext(0, true)));
            RecursiveMock<ISelectProject>()
                .Setup(x => x.IsProjectSelected)
                .Returns(Scheduler.CreateColdObservable(OnNext(0, true)));
            RecursiveMock<IGetUserProfile>()
                .Setup(x => x.IsProfileValid)
                .Returns(Scheduler.CreateColdObservable(OnNext(0, true)));

            GetT();

            Scheduler.AdvanceBy(100);

            Assert.False(T.Save.CanExecute(null));
        }

        [Fact]
        public void PushesValidCredentials()
        {
            // Setup
            RecursiveMock<IValidateLogin>()
                .Setup(x => x.IsLoginValid)
                .Returns(
                    Scheduler.CreateColdObservable(OnNext(0, true))
                );
            var validCredentials = new AppSettings()
            {
                UserName = VALID_USER,
                Password = VALID_PASS
            };
            RecursiveMock<IValidateLogin>()
                .Setup(x => x.ValidCredentials)
                .Returns(
                Scheduler.CreateColdObservable(OnNext(0, validCredentials))
                );

            var validWithDB = new AppSettings()
            {
                UserName = VALID_USER,
                Password = VALID_PASS,
                HomeDBName = VALID_DB
            };
            RecursiveMock<ISelectDatabase>()
                .Setup(x => x.CredentialsWithDatabase)
                .Returns(
                    Scheduler.CreateColdObservable(OnNext(150, validWithDB))
                    );

            GetT();

            //Execute

            Scheduler.AdvanceTo(100);

            Mock<ISelectDatabase>()
                .Verify(x => x.ValidCredentials.OnNext(It.Is<AppSettings>(s => s.UserName == VALID_USER && s.Password == VALID_PASS)));
            Mock<ISelectProject>()
                .Verify(x => x.CredentialsWithDatabase.OnNext(It.IsAny<AppSettings>()), Times.Never());
            Mock<IGetUserProfile>()
                .Verify(x => x.CredentialsWithDatabase.OnNext(It.IsAny<AppSettings>()), Times.Never());

            Scheduler.AdvanceTo(200);

            Mock<ISelectProject>()
                .Verify(x => x.CredentialsWithDatabase.OnNext(It.Is<AppSettings>(s => s.UserName == VALID_USER && s.Password == VALID_PASS && s.HomeDBName == VALID_DB)));
           

            
        }
    }
}
