using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiversityPhone.ViewModels;
using DiversityService.Model;
using DiversityPhone.Messages;

namespace DiversityPhone.TestLibrary
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für EventSeriesVMTest
    /// </summary>
    [TestClass]
    public class EventSeriesVMTest : ViewModelTestBase
    {
        EventSeriesVM _target;
        public EventSeriesVMTest()
        {
            _target = _moqer.Resolve<EventSeriesVM>();            
        }


        [TestMethod]
        public void Clicking_On_The_Series_Should_Send_Selection_Message()
        {
            //Setup
            var selectionSent = false;
            _messenger.Listen<EventSeries>(MessageContracts.SELECT)
                .Subscribe(_ => selectionSent = true);

            //Execute
            _target.SelectSeries.Execute(null);
            passTime();

            //Assert
            Assert.IsTrue(selectionSent, "Selection Message was not sent!");
        }
    }
}
