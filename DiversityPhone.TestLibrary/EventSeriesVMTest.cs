using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReactiveUI;
using AutoMoq;
using DiversityPhone.ViewModels;
using System.Reactive.Concurrency;

namespace DiversityPhone.TestLibrary
{
    [TestClass]
    public class EventSeriesVMTest
    {
        EventSeriesViewModel _target;
        AutoMoqer _moqer;
        public EventSeriesVMTest ()	
        {
            RxApp.DeferredScheduler = new EventLoopScheduler();
            _moqer = new AutoMoqer();


            _target = _moqer.Resolve<EventSeriesViewModel>();
        }

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
