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
        protected ReactiveAsyncCommand Executor {get; private set;}
        private IObservable<object> _StartObs, _CompletedObs, _ErrorObs;
        private ISubject<bool> _IsBusySubject = new ReplaySubject<bool>(1);

        private bool _IsBusyStore = false;
        private bool _IsBusy
        {
            get
            {
                return _IsBusyStore;
            }
            set
            {
                lock(this)
                {
                    _IsBusyStore = value;
                    _IsBusySubject.OnNext(value);
                }
            }
        }


        private ISubject<string> _progressMessageSubject = new ReplaySubject<string>(1);
        private ISubject<Exception> _ErrorSubject;

        public BackgroundTask()
        {
            Executor = new ReactiveAsyncCommand();
            Executor.RegisterAsyncAction(arg => Run(arg));

            var start = Executor.AsyncStartedNotification
                .Do(_ => _IsBusy = true)
                .Select(_ => Invocation.Argument)
                .Publish();
            start.Connect();
            _StartObs = start;
           
            var complete = Executor.AsyncCompletedNotification
                .Do(_ => _IsBusy = false)
                .Select(_ => Invocation.Argument)
                .Publish();
            complete.Connect();
            _CompletedObs = complete;


            _ErrorSubject = new Subject<Exception>();
            var error = Executor.ThrownExceptions
                .Merge(_ErrorSubject)
                .Do(_ => _IsBusy = false)
                .Select(_ => Invocation.Argument)
                .Publish();
            error.Connect();
            _ErrorObs = error;
        }

        public void Invoke(BackgroundTaskInvocation inv)
        {
            if (Executor.CanExecute(null))
            {                
                Invocation = inv;
                try
                {
                    if (inv.Argument != null)
                        saveArgumentToState(inv.Argument);
                    else
                        inv.Argument = getArgumentFromState();

                    Executor.Execute(inv.Argument);
                }
                catch (Exception ex) 
                {
                    _ErrorSubject.OnNext(ex);
                } 
            }            
        }

        public void StoreInvocation()
        {               
            Cancel();            
        }


        public void CleanupAfter(BackgroundTaskInvocation inv)
        {
            Invocation = inv;
            try
            {
                if (inv.Argument != null)
                    saveArgumentToState(inv.Argument);
                else
                    inv.Argument = getArgumentFromState();
                Cleanup(inv.Argument);
            }
            catch (Exception ex)
            {
                //Ignore (assumed to be cleaned up)
            }
        }

        

        #region IBackgroundTask
        public IObservable<object> AsyncCompletedNotification
        {
            get { return _CompletedObs; }
        }

        public IObservable<object> AsyncErrorNotification
        {
            get { return _ErrorObs; }
        }

        public IObservable<object> AsyncStartedNotification
        {
            get { return _StartObs; }
        }

        public IObservable<bool> CanExecuteObservable
        {
            get { return Executor.CanExecuteObservable; }
        }

        public IObservable<bool> BusyObservable
        {
            get { return _IsBusySubject; }
        }

        public bool IsBusy { get { return _IsBusy; } }

        public object CurrentArguments
        {
            get 
            {
                if (IsBusy && Invocation != null)
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
