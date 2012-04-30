using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Subjects;
using Funq;
using ReactiveUI.Xaml;

namespace DiversityPhone.Services
{
    public abstract class BackgroundTask : IBackgroundTask
    {
        public BackgroundTaskInvocation Invocation {get; set;}

        protected ReactiveAsyncCommand Executor {get; private set;}
        protected Dictionary<string, object> State { get { return Invocation.State; } }

        public BackgroundTask()
        {
            Executor = new ReactiveAsyncCommand();
            Executor.RegisterAsyncAction(arg => Run(arg));
        }

        public void Invoke(BackgroundTaskInvocation inv)
        {
            if(Executor.CanExecute(null))
            {
                Invocation = inv;
                Executor.Execute(inv.Argument);
            }
        }

        /// <summary>
        /// Indicates whether this type of Task can resume.
        /// </summary>
        public abstract bool CanResume { get; }      
        
       
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


        public abstract void Cleanup(BackgroundTaskInvocation inv);




        public IObservable<System.Reactive.Unit> AsyncCompletedNotification
        {
            get { return Executor.AsyncCompletedNotification; }
        }

        public IObservable<Exception> AsyncErrorNotification
        {
            get { return Executor.AsyncErrorNotification; }
        }

        public IObservable<System.Reactive.Unit> AsyncStartedNotification
        {
            get { return Executor.AsyncStartedNotification; }
        }

        public IObservable<bool> CanExecuteObservable
        {
            get { return Executor.CanExecuteObservable; }
        }

        public IObservable<int> ItemsInflight
        {
            get { return Executor.ItemsInflight; }
        }
    }
}
