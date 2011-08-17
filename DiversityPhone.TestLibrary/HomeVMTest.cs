using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiversityPhone.ViewModels;
using AutoMoq;
using DiversityPhone.Services;
using ReactiveUI;
using System.Reactive.Concurrency;

namespace DiversityPhone.TestLibrary
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für HomeVMTest
    /// </summary>
    [TestClass]
    public class HomeVMTest : ViewModelTestBase
    {
        HomeViewModel _target;
        
        public HomeVMTest()
        {
            _target = _moqer.Resolve<HomeViewModel>();
        }       



        [TestMethod]
        public void ClickOnSettingsShouldNavigateToPage()
        {
            //Execute
            _target.Settings.Execute(null);
            passTime();


            //Assert
            _moqer.GetMock<INavigationService>()
                .Verify(x => x.Navigate(Page.Settings));
        }


    }
}
