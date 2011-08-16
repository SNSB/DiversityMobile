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
    [TestClass]
    public class EditESVMTest : ViewModelTestBase
    {
        EditESVM _target;
        public EditESVMTest()
        {
            _target = _moqer.Resolve<EditESVM>();
        }

        [TestMethod]
        public void VM_should_recieve_ES_to_Edit()
        {
            EventSeries message = new EventSeries { Description = "Test" };

            Assert.AreNotEqual(message.Description, _target.Description);
            Assert.AreNotEqual(message, _target.Model);

            _messenger.SendMessage<EventSeries>(message,MessageContracts.EDIT);

            passTime();

            Assert.AreEqual(message.Description, _target.Description);
            Assert.AreEqual(message, _target.Model);


        }
    }
}
