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
    public class HomeVMTest
    {
        HomeViewModel _target;
        AutoMoqer _moqer;
        public HomeVMTest()
        {
            RxApp.DeferredScheduler = new EventLoopScheduler();
            _moqer = new AutoMoqer();
            

            _target = _moqer.Resolve<HomeViewModel>();
        }
        

        #region Zusätzliche Testattribute
        //
        // Sie können beim Schreiben der Tests folgende zusätzliche Attribute verwenden:
        //
        // Verwenden Sie ClassInitialize, um vor Ausführung des ersten Tests in der Klasse Code auszuführen.
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Verwenden Sie ClassCleanup, um nach Ausführung aller Tests in einer Klasse Code auszuführen.
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen. 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void ClickOnSettingsShouldNavigateToPage()
        {
            _target.Settings.Execute(null);

            _moqer.GetMock<INavigationService>()
                .Verify(x => x.Navigate(Page.Settings));
        }


    }
}
