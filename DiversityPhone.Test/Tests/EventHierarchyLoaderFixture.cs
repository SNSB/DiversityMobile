using DiversityPhone.Model;
using DiversityPhone.ViewModels;
using Microsoft.Reactive.Testing;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Xunit;

namespace DiversityPhone.Test.Tests
{
    [Trait("Component", "EventHierarchyLoader")]
    public class EventHierarchyLoaderFixture : DiversityTestBase<EventHierarchyLoader>
    {
        private const int EVENT_KEY = 1000;
        private const int SERIES_ID = 1000;
        private readonly int? NOSERIES_ID = null;
        private const int LOCAL_SERIES_ID = 1001;

        private readonly TestResources Resources;

        public EventHierarchyLoaderFixture()
        {
            Resources = new TestResources();
        }

        private void SetupHierarchy()
        {
            Service.Setup(x => x.GetEventsByLocality(It.IsAny<string>()))
                .Returns(Return(Enumerable.Repeat(Resources.Event, 1)));

            Service.Setup(x => x.GetEventSeriesByID(Resources.EventSeries.CollectionSeriesID.Value))
                .Returns(Return(Resources.EventSeries));
            Service.Setup(x => x.GetEventSeriesLocalizations(Resources.EventSeries.CollectionSeriesID.Value))
                .Returns(Return(Enumerable.Empty<Localization>()));

            Service.Setup(x => x.GetSpecimenForEvent(Resources.Event.CollectionEventID.Value))
                .Returns(Return(Enumerable.Repeat(Resources.Specimen, 1)));
            Service.Setup(x => x.GetEventProperties(Resources.Event.CollectionEventID.Value))
                .Returns(Return(Enumerable.Empty<EventProperty>()));

            Service.Setup(x => x.GetIdentificationUnitsForSpecimen(Resources.Specimen.CollectionSpecimenID.Value))
                .Returns(Return(Enumerable.Repeat(Resources.Unit, 1)));
            Service.Setup(x => x.GetAnalysesForIU(Resources.Unit.CollectionUnitID.Value))
                .Returns(Return(Enumerable.Empty<IdentificationUnitAnalysis>()));
        }

        [Fact]
        public void DoesntLoadUnmappedSeriesFromStorage()
        {
            GetT();
            T.getOrDownloadSeries(SERIES_ID);

            // Assert
            Storage.Verify(s => s.get<EventSeries>(SERIES_ID), Times.Never(), "Loading Series when it doesn't exist.");
        }

        [Fact]
        public void LoadsMappedSeriesFromStorage()
        {
            // Setup
            Mappings.Setup(m => m.ResolveToLocalKey(DBObjectType.EventSeries, SERIES_ID)).Returns(LOCAL_SERIES_ID);

            // Execute
            GetT();
            T.getOrDownloadSeries(SERIES_ID);

            // Assert
            Storage.Verify(s => s.get<EventSeries>(LOCAL_SERIES_ID));
        }

        [Fact]
        public void ReturnsNoEventSeries()
        {
            // Setup

            // Execute
            GetT();
            var series = T.getOrDownloadSeries(NOSERIES_ID);

            // Assert
            ReactiveAssert.AreElementsEqual(series, Observable.Return(NoEventSeriesMixin.NoEventSeries));
        }

        [Fact]
        public void DownloadStartsOnlyOnSubscribing()
        {
            GetT();
            var download = T.downloadAndStoreDependencies(new Event() { CollectionEventID = 0 });

            Service.Verify(s => s.GetEventProperties(It.IsAny<int>()), Times.Never());

            var subscrption = download.Subscribe();

            Service.Verify(s => s.GetEventProperties(It.IsAny<int>()), Times.Once());
        }

        [Fact]
        public void DownloadStopsWhenUnsubscribing()
        {
            //Setup
            var specimen = new List<Specimen>() { Resources.Specimen } as IList<Specimen>;
            var specimenObs = Scheduler.CreateHotObservable(
                OnNext(100, specimen),
                OnCompleted(specimen, 100)
                    );
            Service.Setup(s => s.GetSpecimenForEvent(Resources.Event.CollectionEventID.Value)).Returns(specimenObs);

            //Execute
            var subscription = T.downloadAndStoreDependencies(Resources.Event).Subscribe();

            subscription.Dispose();

            Scheduler.Start();

            //Assert

            Service.Verify(s => s.GetIdentificationUnitsForSpecimen(It.IsAny<int>()), Times.Never());
        }

        [Fact]
        public void InsertsEachObjectOnlyOnce()
        {
            SetupHierarchy();

            T.downloadAndStoreDependencies(Resources.Event).Subscribe();

            Scheduler.Start();

            Storage.Verify(x => x.add<EventSeries>(Resources.EventSeries), Times.Once());
            Storage.Verify(x => x.add<Event>(It.IsAny<Event>()), Times.Once());
            Storage.Verify(x => x.add<Specimen>(Resources.Specimen), Times.Once());
            Storage.Verify(x => x.add<IdentificationUnit>(Resources.Unit), Times.Once());
        }

        [Fact]
        public void DoesNotInsertEventObjectPassedAsParameter()
        {
            SetupHierarchy();

            T.downloadAndStoreDependencies(Resources.Event).Subscribe();

            Scheduler.Start();

            Storage.Verify(x => x.add<Event>(It.Is<Event>(ev => !object.ReferenceEquals(ev, Resources.Event) && ev.CollectionEventID == Resources.Event.CollectionEventID)), Times.Once());
        }

        [Fact]
        public void ForeignKeyDependenciesCorrectlySetup()
        {
            SetupHierarchy();

            T.downloadAndStoreDependencies(Resources.Event).Subscribe();

            Scheduler.Start();

            Storage.Verify(x => x.add<Event>(It.Is<Event>(ev => ev.SeriesID == Resources.EventSeries.SeriesID)));
            Storage.Verify(x => x.add<Specimen>(It.Is<Specimen>(s => s.EventID == Resources.Event.EventID)));
            Storage.Verify(x => x.add<IdentificationUnit>(It.Is<IdentificationUnit>(iu => iu.SpecimenID == Resources.Specimen.SpecimenID)));
        }

        [Fact]
        public void DoesntAssumeListSemanticsFromEnumerable()
        {
            SetupHierarchy();

            Service.Setup(x => x.GetIdentificationUnitsForSpecimen(Resources.Specimen.CollectionSpecimenID.Value))
                .Returns(Return(new SingleUseEnumerable<IdentificationUnit>(Enumerable.Repeat(Resources.Unit, 1))));

            T.downloadAndStoreDependencies(Resources.Event).Subscribe();

            Scheduler.Start();

            Storage.Verify(x => x.add<IdentificationUnit>(It.Is<IdentificationUnit>(iu => iu.SpecimenID == Resources.Specimen.SpecimenID)));
        }
    }
}