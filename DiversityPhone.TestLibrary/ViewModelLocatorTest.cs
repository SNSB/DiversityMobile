using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DiversityPhone.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoMoq;
using Funq;
using DiversityPhone.Services;
using ReactiveUI;

namespace DiversityPhone.TestLibrary
{
    [TestClass]
    public class ViewModelLocatorTest : ViewModelTestBase
    {
        ViewModelLocator _target;        
        public ViewModelLocatorTest()
        {
            _target = new ViewModelLocator(_ioc);
        }

        [TestMethod]
        public void VMsResolving()
        {
            Assert.IsNotNull(_target.Home);
            Assert.IsNotNull(_target.EventSeries);
            Assert.IsNotNull(_target.EditES);
        }
    }
}
