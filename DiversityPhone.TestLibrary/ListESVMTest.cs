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
    public class ListESVMTest : ViewModelTestBase
    {
        ListESVM _target;        
        public ListESVMTest()
        {
            _target = _moqer.Resolve<ListESVM>();
        }

        [TestMethod]
        public void AddSeriesShouldShowEditPage()
        {

            bool editMessageSent = false;
            _messenger.Listen<EventSeries>(MessageContracts.EDIT)
                .Subscribe(es => editMessageSent |= (es != null));

            _target.AddSeries.Execute(null);

            _scheduler.AdvanceBy(1000);
            
            _moqer.GetMock<INavigationService>()
                .Verify(nav => nav.Navigate(Page.EditEventSeries));
            Assert.IsTrue(editMessageSent);
            
        }

        [TestMethod]
        public void Saving_Edited_ES_should_update_view()
        {
            //Setup
            EventSeries es = _moqer.GetMock<EventSeries>().Object;
            var storage = _moqer.GetMock<IOfflineStorage>();
            var esList = _moqer.GetMock<IList<EventSeries>>().Object;

            storage.Setup(s => s.getAllEventSeries()).Returns(esList);

            //Execute
            _messenger.SendMessage<EventSeries>(es, MessageContracts.SAVE);
            passTime();


            Assert.AreSame(esList, _target.SeriesList);

            storage.Verify(s => s.getAllEventSeries());
            storage.Verify(s => s.addEventSeries(es));
            
        }
    }
}
