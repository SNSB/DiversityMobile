using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiversityPhone.ViewModels;
using AutoMoq;
using System.Reactive.Concurrency;
using ReactiveUI;
using DiversityPhone.Services;
using Moq;
using DiversityService.Model;
using DiversityPhone.Messages;

namespace DiversityPhone.TestLibrary
{
    [TestClass]
    public class EventSeriesViewModelTest : ViewModelTestBase
    {
        EventSeriesViewModel _target;        
        public EventSeriesViewModelTest()
        {
            _target = _moqer.Resolve<EventSeriesViewModel>();
        }

        [TestMethod]
        public void AddSeriesShouldShowEditPage()
        {

            bool editMessageSent = false;
            _messenger.Listen<EventSeries>(MessageContracts.EDIT)
                .Subscribe(es => editMessageSent |= (es != null));

            _target.AddSeries.Execute(null);
            
            _moqer.GetMock<INavigationService>()
                .Verify(nav => nav.Navigate(Page.EditEventSeries));
            Assert.IsTrue(editMessageSent);
            
        }
    }
}
