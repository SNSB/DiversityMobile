namespace DiversityPhone.Test.Tests
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using DiversityPhone.ViewModels;
    using FluentAssertions;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using Xunit;

    [Trait("ViewModels", "Download")]
    public class DownloadVMFixture : DiversityTestBase<DownloadVM>
    {
        private IEnumerable<Event> SearchResult = new List<Event>()
        {
            new Event(),
            new Event(),
            new Event()
        };

        private const string TEST_QUERY = "TestEvent";

        public DownloadVMFixture()
        {
        }

        [Fact]
        public void CanOnlySearchWhenOnline()
        {
            // Setup
            var connectivityObs = Scheduler.CreateHotObservable(
                    OnNext(100, ConnectionStatus.None),
                    OnNext(200, ConnectionStatus.Wifi),
                    OnNext(300, ConnectionStatus.MobileBroadband)
                    );
            Connectivity.Setup(c => c.Status()).Returns(connectivityObs);

            //Execute
            GetT();

            T.SearchEvents.CanExecute(TEST_QUERY).Should().BeFalse();
            T.IsOnlineAvailable.Should().BeFalse();

            Scheduler.AdvanceTo(100);

            T.SearchEvents.CanExecute(TEST_QUERY).Should().BeFalse();
            T.IsOnlineAvailable.Should().BeFalse();

            Scheduler.AdvanceTo(200);

            T.SearchEvents.CanExecute(TEST_QUERY).Should().BeTrue();
            T.IsOnlineAvailable.Should().BeTrue();

            Scheduler.AdvanceTo(300);

            T.SearchEvents.CanExecute(TEST_QUERY).Should().BeFalse();
            T.IsOnlineAvailable.Should().BeFalse();
        }

        [Fact]
        public void SearchQueriesTheService()
        {
            // Setup
            Connectivity.Setup(c => c.Status()).Returns(ReturnAndNever(ConnectionStatus.Wifi));
            Service.Setup(s => s.GetEventsByLocality(It.IsAny<string>())).Returns(Observable.Empty<IEnumerable<Event>>());
            Notifications.SetReturnsDefault(Disposable.Empty as IDisposable);

            //Execute
            GetT();
            Scheduler.Start();

            T.SearchEvents.Execute(TEST_QUERY);

            //Assert

            Service.Verify(x => x.GetEventsByLocality(TEST_QUERY));
        }
    }
}