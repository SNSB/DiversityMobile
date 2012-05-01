using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using DiversityPhone.Messages;
using System.Collections.Generic;
using Funq;
using System.Linq;
using System.Reactive.Linq;


namespace DiversityPhone.Services
{
    public class BackgroundService : Dictionary<string, BackgroundTask>, IBackgroundService
    {
        IMessageBus Messenger;        

        bool shuttingDown = false;
        Queue<BackgroundTaskInvocation> waitingTasks = new Queue<BackgroundTaskInvocation>();
        BackgroundTaskInvocation runningTask;        

        public BackgroundService(Container ioc)
        {
            Messenger = ioc.Resolve<IMessageBus>();           
        }  
        

        public void startTask<T>(object arg) where T : BackgroundTask
        {
            lock (this)
            {
                waitingTasks.Enqueue(new BackgroundTaskInvocation(typeof(T)) { Argument = arg });
            }
            nextTask();
        }

        private void nextTask()
        {
            if (shuttingDown)
                return;

            lock (this)
            {
                while (runningTask == null && waitingTasks.Count > 0)
                {
                    runningTask = waitingTasks.Dequeue();
                    BackgroundTask task;
                    if(this.TryGetValue(runningTask.Type, out task))
                    {
                        if (task.CurrentItemsInFlight == 0)
                        {
                            if (!runningTask.HasStarted || task.CanResume)
                            {
                                task.ItemsInflight.StartWith(0)
                                    .Zip(task.ItemsInflight, (prev, now) => prev == 1 && now == 0)
                                    .Where(free => free && !shuttingDown)
                                    .Take(1)
                                    .Subscribe(_ => taskFinished());

                                runningTask.WasCancelled = false;
                                runningTask.HasStarted = true;
                                
                                task.Invoke(runningTask);
                            }
                            else // Cleanup and restart
                            {
                                task.CleanupAfter(runningTask);

                                runningTask.HasStarted = false;
                                waitingTasks.Enqueue(runningTask);
                                runningTask = null;
                            }
                        }
                        else
                        {
                            waitingTasks.Enqueue(runningTask);
                            runningTask = task.Invocation;
                        }
                    }                 
                }
            }                      
        }

        private void taskFinished()
        {
            lock (this)
            {
                runningTask = null;
            }
            nextTask();
        }



        public IEnumerable<BackgroundTaskInvocation> shutdown()
        {
            lock (this)
            {
                shuttingDown = true;
                if (runningTask != null)
                {
                    runningTask.WasCancelled = true;
                    var bgtask = this[runningTask.Type];
                    bgtask.CancelInvocation();                    
                    waitingTasks.Enqueue(runningTask);
                    runningTask = null;
                }        
                return waitingTasks;                      
            }
        }

        public void initialize(IEnumerable<BackgroundTaskInvocation> backlog)
        {
            lock (this)
            {
                waitingTasks.Clear();
                foreach (var task in backlog)
	            {
                    waitingTasks.Enqueue(task);		 
	            }
                shuttingDown = false;
            }            
            nextTask();
        }                    

        public T getTaskObject<T>() where T : BackgroundTask
        {
            if (this.ContainsKey(typeof(T).ToString()))
                return (T)this[typeof(T).ToString()];
            else
                return null;
        }
    }
}
