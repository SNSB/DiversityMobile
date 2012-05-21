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
        /// The State Dictionary for this Invocation that can be used to store
        /// - The Argument of this Invocation
        /// - The partial results of this Invocation for the purpose of resuming it
        /// - etc.
        /// </summary>
        protected Dictionary<string, string> State { get { return Invocation.State; } }       

        /// <summary>
        /// Reports a given status string via the AsyncProgressMessages Property
        /// </summary>
        /// <param name="message">status string</param>
        protected void reportProgress(string message)
        {
            _progressMessageSubject.OnNext(message);
        }

        /// <summary>
        /// Indicates whether this type of Task can resume.
        /// Tasks that cannot resume are cleaned up before reinvocation
        /// those that can are expected to handle resuming on their own
        /// </summary>
        public abstract bool CanResume { get; }

        /// <summary>
        /// Saves the argument Object to the State Dictionary, so that it can be restored later.
        /// </summary>
        /// <param name="arg">Invocation Argument</param>
        protected abstract void saveArgumentToState(object arg);

        /// <summary>
        /// Restores the argument Object from the State Dictionary, after it has been stored there
        /// </summary>
        /// <returns>Invocation Argument</returns>
        protected abstract object getArgumentFromState();

        /// <summary>
        /// Task Entry Point for the executing background Thread
        /// </summary>        
        protected abstract void Run(object arg);

        /// <summary>
        /// Cancels the Task
        /// When this method returns, the cancellation must have been processed
        /// </summary>
        protected abstract void Cancel();

        /// <summary>
        /// Cleans up the effects of partial invocation of the task
        /// After this has run, it must be safe to restart the invocation.
        /// </summary>
        /// <param name="arg">Invocation Argument</param>
        protected abstract void Cleanup(object arg); 
        

        #endregion

        public BackgroundTaskInvocation Invocation { get; private set; }

        private ISubject<object> _cleanupSubject = new Subject<object>();
        protected ReactiveAsyncCommand Executor {get; private set;}
        private IObservable<int> _ItemsInFlightObs;
        private int _ItemsInFlight = 0;
        

        private ISubject<string> _progressMessageSubject = new ReplaySubject<string>(1);

        public BackgroundTask()
        {
            Executor = new ReactiveAsyncCommand();
            Executor.RegisterAsyncAction(arg => Run(arg));

            var inflight = Executor.ItemsInflight.Do(items => _ItemsInFlight = items).Replay(1);
            inflight.Connect();
            _ItemsInFlightObs = inflight;                      
        }

        public void Invoke(BackgroundTaskInvocation inv)
        {
            if (Executor.CanExecute(null))
            {                
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
            get { return Executor.AsyncCompletedNotification.Select(_ => Invocation.Argument); }
        }

        public IObservable<object> AsyncErrorNotification
        {
            get { return Executor.AsyncErrorNotification.Select(_ => Invocation.Argument); }
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
