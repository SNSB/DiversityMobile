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
            //Setup
            bool editMessageSent = false;
            _messenger.Listen<EventSeries>(MessageContracts.EDIT)
                .Subscribe(es => editMessageSent |= (es != null));

            //Execute
            _target.AddSeries.Execute(null);
            passTime();
            
            //Assert
            _moqer.GetMock<INavigationService>()
                .Verify(nav => nav.Navigate(Page.EditEventSeries));
            Assert.IsTrue(editMessageSent);
            
        }

        [TestMethod]
        public void Saving_Edited_ES_should_save_and_update_view()
        {
            //Setup
            EventSeries es = _moqer.GetMock<EventSeries>().Object;
            var storage = _moqer.GetMock<IOfflineStorage>();
            var esList = _moqer.GetMock<IList<EventSeries>>();
            esList.Setup(list => list.Count).Returns(321);

            storage.Setup(s => s.getAllEventSeries()).Returns(esList.Object);

            //Execute
            _messenger.SendMessage<EventSeries>(es, MessageContracts.SAVE);
            passTime();

            //Assert
            Assert.AreEqual<int>(esList.Object.Count, _target.SeriesList.Count);

            storage.Verify(s => s.getAllEventSeries());
            storage.Verify(s => s.addEventSeries(es));
            
        }

        [TestMethod]
        public void Selecting_an_ES_should_navigate_to_next_Page()
        {
            //Setup
            var es = _moqer.GetMock<EventSeries>().Object;

            //Execute
            _messenger.SendMessage<EventSeries>(es, MessageContracts.SELECT);
            passTime();

            //Assert
            _moqer.GetMock<INavigationService>()
                .Verify(nav => nav.Navigate(Page.ListEvents));
            
        }        
    }
}
