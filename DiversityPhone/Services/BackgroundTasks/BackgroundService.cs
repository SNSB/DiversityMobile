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
    public class BackgroundService : Dictionary<Type, BackgroundTask>, IBackgroundService
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
            lock (this)
            {
                if (runningTask == null && waitingTasks.Count > 0)
                {
                    runningTask = waitingTasks.Dequeue();
                    BackgroundTask task;
                    if(this.TryGetValue(runningTask.Type, out task))
                    {
                        if (!runningTask.HasStarted || task.CanResume)
                        {
                            runningTask.HasStarted = true;
                            task.Invoke(runningTask);
                        }
                        else
                        {
                            task.Cleanup(runningTask);
                            runningTask = null;
                        }
                    }                 
                }
            }
            if (runningTask == null)
                nextTask();            
        }



        public IEnumerable<BackgroundTaskInvocation> shutdown()
        {
            lock (this)
            {
                shuttingDown = true;
                if (runningTask != null)
                {
                    var bgtask = this[runningTask.Type];
                    bgtask.Cancel();                                        
                        
                }        
                return waitingTasks;                      
            }
        }

        public void initialize(IEnumerable<BackgroundTaskInvocation> backlog)
        {
            lock (this)
            {
                foreach (var task in backlog)
	            {
                    waitingTasks.Enqueue(task);		 
	            }                
            }
            nextTask();
        }                    

        public T getTaskObject<T>() where T : BackgroundTask
        {
            if (this.ContainsKey(typeof(T)))
                return (T)this[typeof(T)];
            else
                return null;
        }
    }
}
