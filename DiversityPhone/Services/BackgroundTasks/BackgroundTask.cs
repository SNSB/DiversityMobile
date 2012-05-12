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
        #region subclass Interface
        /// <summary>
        /// Indicates whether this type of Task can resume.
        /// </summary>
        public abstract bool CanResume { get; }

        public bool Cancelled { get; private set; }

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
        protected abstract void Cancel();


        protected abstract void Cleanup(object arg);

        public BackgroundTaskInvocation Invocation {get; private set;}

        protected void reportProgress(string message)
        {
            _progressMessageSubject.OnNext(message);
        }

        #endregion

        private ISubject<object> _cleanupSubject = new Subject<object>();
        protected ReactiveAsyncCommand Executor {get; private set;}
        private IObservable<int> _ItemsInFlightObs;
        private int _ItemsInFlight = 0;
        protected Dictionary<string, string> State { get { return Invocation.State; }}

        private ISubject<string> _progressMessageSubject = new ReplaySubject<string>(1);

        public BackgroundTask()
        {
            Executor = new ReactiveAsyncCommand();
            Executor.RegisterAsyncAction(arg => Run(arg));

            var inflight = Executor.ItemsInflight.Do(items => _ItemsInFlight = items).Replay(1);
            inflight.Connect();
            _ItemsInFlightObs = inflight;

            Cancelled = false;
        }

        public void Invoke(BackgroundTaskInvocation inv)
        {
            if (Executor.CanExecute(null))
            {
                Cancelled = false;
                Invocation = inv;
                if (inv.Argument != null)
                    saveArgumentToState(inv.Argument);
                else
                    inv.Argument = getArgumentFromState();

                Executor.Execute(inv.Argument);                
            }            
        }

        public void CancelInvocation()
        {            
            Cancel();
            Cancelled = true;
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

        

        #region IBackgroundTask
        public IObservable<object> AsyncCompletedNotification
        {
            get { return Executor.AsyncCompletedNotification.Where(_ => !Invocation.WasCancelled).Select(_ => Invocation.Argument); }
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
            get { return _ItemsInFlightObs; }
        }

        public int CurrentItemsInFlight { get { return _ItemsInFlight; } }

        public object CurrentArguments
        {
            get 
            {
                if (CurrentItemsInFlight > 0 && Invocation != null)
                    return Invocation.Argument;
                else
                    return null;
            }
        }


        public IObservable<string> AsyncProgressMessages
        {
            get { return _progressMessageSubject; }
        }
        #endregion
    }
}
