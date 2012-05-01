using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Subjects;
using Funq;
using ReactiveUI.Xaml;
using System.Reactive.Linq;
using ReactiveUI;

namespace DiversityPhone.Services
{
    public abstract class BackgroundTask : IBackgroundTask
    {
        public BackgroundTaskInvocation Invocation {get; set;}

        private ISubject<object> _cleanupSubject = new Subject<object>();
        protected ReactiveAsyncCommand Executor {get; private set;}
        private ObservableAsPropertyHelper<int> _currentInflight;
        protected Dictionary<string, string> State { get { return Invocation.State; }}

        public BackgroundTask()
        {
            Executor = new ReactiveAsyncCommand();
            Executor.RegisterAsyncAction(arg => Run(arg));

            _currentInflight = new ObservableAsPropertyHelper<int>(Executor.ItemsInflight, (i) => { }, 0);
        }

        public void Invoke(BackgroundTaskInvocation inv)
        {            
            Invocation = inv;
            if (inv.Argument != null)
                saveArgumentToState(inv.Argument);
            else
                inv.Argument = getArgumentFromState();

            Executor.Execute(inv.Argument);            
        }

        public void CleanupAfter(BackgroundTaskInvocation inv)
        {
            Invocation = inv;
            if (inv.Argument != null)
                saveArgumentToState(inv.Argument);
            else
                inv.Argument = getArgumentFromState();
            Cleanup(inv.Argument);
            _cleanupSubject.OnNext(inv.Argument);
        }

        /// <summary>
        /// Indicates whether this type of Task can resume.
        /// </summary>
        public abstract bool CanResume { get; }

        protected abstract void saveArgumentToState(object arg);

        protected abstract object getArgumentFromState();
        
       
        /// <summary>
        /// Runs the Task on a background Thread, returing an Observable that is used to monitor Progress
        /// </summary>
        /// <returns></returns>
        protected abstract void Run(object arg);

        /// <summary>
        /// Cancels the Task
        /// When this method returns, the cancellation must have been processed
        /// </summary>
        public abstract void Cancel();
       

        protected abstract void Cleanup(object arg);


        public IObservable<object> AsyncCompletedNotification
        {
            get { return Executor.AsyncCompletedNotification.Select(_ => Invocation.Argument); }
        }

        public IObservable<object> AsyncCleanupNotification
        {
            get { return _cleanupSubject; }
        }

        public IObservable<object> AsyncStartedNotification
        {
            get { return Executor.AsyncStartedNotification.Select(_ => Invocation.Argument); }
        }

        public IObservable<bool> CanExecuteObservable
        {
            get { return Executor.CanExecuteObservable; }
        }

        public IObservable<int> ItemsInflight
        {
            get { return Executor.ItemsInflight; }
        }


        public object CurrentArguments
        {
            get 
            {
                if (_currentInflight.Value > 0 && Invocation != null)
                    return Invocation.Argument;
                else
                    return null;
            }
        }
    }
}
