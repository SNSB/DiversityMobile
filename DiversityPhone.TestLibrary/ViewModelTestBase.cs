using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMoq;
using ReactiveUI;
using System.Reactive.Concurrency;
using Funq;
using DiversityPhone.Services;

namespace DiversityPhone.TestLibrary
{
    public class ViewModelTestBase
    {        
        protected AutoMoqer _moqer;
        protected Container _ioc;
        protected IMessageBus _messenger = new ReactiveUI.MessageBus();
        protected EventLoopScheduler _scheduler = new EventLoopScheduler();
        public ViewModelTestBase()
        {
            RxApp.DeferredScheduler = _scheduler;
            _moqer = new AutoMoqer();
            _moqer.SetInstance<IMessageBus>(_messenger);
            _ioc = new Container();


            _ioc.Register<IOfflineStorage>(_moqer.GetMock<IOfflineStorage>().Object);
            _ioc.Register<INavigationService>(_moqer.GetMock<INavigationService>().Object);
            _ioc.Register<IMessageBus>(_messenger);
            

           
        }
    }
}
