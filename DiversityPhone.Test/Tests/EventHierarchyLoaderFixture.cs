using DiversityPhone.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using DiversityPhone.Model;
using Microsoft.Reactive.Testing;
using FluentAssertions;
using System.Reactive.Linq;

namespace DiversityPhone.Test.Tests
{
    [Trait("Component","EventHierarchyLoader")]
    public class EventHierarchyLoaderFixture : DiversityTestBase<EventHierarchyLoader>
    {
        const int EVENT_KEY = 1000;
        const int SERIES_ID = 1000;
        readonly int? NOSERIES_ID = null;
        const int LOCAL_SERIES_ID = 1001;

        public EventHierarchyLoaderFixture()
        {
            
        }

        [Fact]
        public void getOrDownloadSeries_should_not_load_unmapped_series()
        {
            GetT();
            T.getOrDownloadSeries(SERIES_ID);

            // Assert
            Storage.Verify(s => s.get<EventSeries>(SERIES_ID), Times.Never(), "Loading Series when it doesn't exist.");            
        }

        [Fact]
        public void getOrDownloadSeries_should_load_mapped_series()
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
        public void getOrDownloadSeries_should_return_noeventseries()
        {
            // Setup

            // Execute
            GetT();
            var series = T.getOrDownloadSeries(NOSERIES_ID);

            // Assert
            ReactiveAssert.AreElementsEqual(series, Observable.Return(EventSeries.NoEventSeries));
        }

       

    }
}
