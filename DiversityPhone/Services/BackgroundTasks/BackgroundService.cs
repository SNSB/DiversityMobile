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
    public class BackgroundService :  IBackgroundService
    {
        IMessageBus Messenger;

        Dictionary<string, BackgroundTask> registry;

        bool suspended = false;
        Queue<BackgroundTaskInvocation> waitingTasks = new Queue<BackgroundTaskInvocation>();
        BackgroundTaskInvocation runningTask;        

        public BackgroundService(Container ioc)
        {
            Messenger = ioc.Resolve<IMessageBus>();
            registry = new Dictionary<string, BackgroundTask>();
        }
        public void registerTask<T>(T task) where T : BackgroundTask
        {
            registry.Add(typeof(T).ToString(), task);
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
            if (suspended)
                return;

            lock (this)
            {
                while (runningTask == null && waitingTasks.Count > 0)
                {
                    runningTask = waitingTasks.Dequeue();
                    BackgroundTask task;
                    if(registry.TryGetValue(runningTask.Type, out task))
                    {
                        if (task.CurrentItemsInFlight == 0)
                        {
                            if (!runningTask.HasStarted || task.CanResume)
                            {
                                Observable.Amb(
                                    task.AsyncCompletedNotification
                                    .Select(_ => true),
                                    task.AsyncErrorNotification
                                    .Select(_ => false)
                                    )
                                    .Subscribe(
                                        success =>
                                        {
                                            if (success)
                                                taskFinished();
                                            else
                                                taskFailed();
                                        });

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

        private void taskFailed()
        {
            registry[runningTask.Type].CleanupAfter(runningTask);
            taskFinished();
        }

        private void taskFinished()
        {
            lock (this)
            {
                runningTask = null;
            }
            nextTask();
        }

        public void suspend()
        {
            suspended = true;
        }

        public IEnumerable<BackgroundTaskInvocation> dumpQueue()
        {
            lock (this)
            {                
                if (runningTask != null)
                {
                    runningTask.WasCancelled = true;
                    var bgtask = registry[runningTask.Type];
                    bgtask.CancelInvocation();                    
                    waitingTasks.Enqueue(runningTask);
                    runningTask = null;
                }        
                return waitingTasks;                      
            }
        }

        public void setQueue(IEnumerable<BackgroundTaskInvocation> backlog)
        {
            lock (this)
            {
                waitingTasks.Clear();
                foreach (var task in backlog)
	            {
                    waitingTasks.Enqueue(task);		 
	            }                
            }       
        }

        public void resume()
        {
            suspended = false;
            nextTask();
        }

        public T getTaskObject<T>() where T : BackgroundTask
        {
            if (registry.ContainsKey(typeof(T).ToString()))
                return (T)registry[typeof(T).ToString()];
            else
                return null;
        }
    }
}
