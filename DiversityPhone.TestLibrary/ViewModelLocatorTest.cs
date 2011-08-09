using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DiversityPhone.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoMoq;
using Funq;
using DiversityPhone.Services;

namespace DiversityPhone.TestLibrary
{
    [TestClass]
    public class ViewModelLocatorTest
    {
        ViewModelLocator _target;
        AutoMoqer _moqer;
        public ViewModelLocatorTest()
        {
            _moqer = new AutoMoqer();
            Container di = new Container();
            di.Register<IOfflineStorage>(_moqer.GetMock<IOfflineStorage>().Object);
            di.Register<INavigationService>(_moqer.GetMock<INavigationService>().Object);

            _target = new ViewModelLocator(di);
        }

        [TestMethod]
        public void HomeResolving()
        {
            Assert.IsNotNull(_target.Home);
        }
    }
}
