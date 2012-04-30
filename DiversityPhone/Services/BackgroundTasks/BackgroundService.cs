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
    public class BackgroundService
    {
        IMessageBus Messenger;
        IBackgroundTaskFactory TaskFactory;

        bool shuttingDown = false;
        Dictionary<BackgroundTaskArguments, BackgroundTask> waitingTasks = new Dictionary<BackgroundTaskArguments, BackgroundTask>();
        BackgroundTaskArguments runningTask;
        IObservable<BackgroundTaskUpdate> runningTaskUpdates;

        public BackgroundService(Container ioc)
        {
            Messenger = ioc.Resolve<IMessageBus>();
            TaskFactory = ioc.Resolve<IBackgroundTaskFactory>();

            Messenger.Listen<BackgroundTaskArguments>(MessageContracts.BACKGROUNDPROCESS_START)                
                .Subscribe(proc => startTask(proc));
            Messenger.Listen<BackgroundTaskArguments>(MessageContracts.BACKGROUNDPROCESS_STOP)                
                .Subscribe(proc => stopTask(proc));
        }

        private void stopTask(BackgroundTaskArguments proc)
        {
            lock (this)
            {
                if (runningTask == proc)
                {
                    var task = waitingTasks[runningTask];
                    task.Cancel();
                    task.Cleanup();
                }               
            }
        }

        public void startTask(BackgroundTaskArguments args)
        {
            lock (this)
            {
                var task = TaskFactory.createTask(args);
                if (task != null)
                    waitingTasks.Add(args, task);
            }
            nextTask();
        }

        private void nextTask()
        {            
            lock (this)
            {
                if (runningTask == null && waitingTasks.Count > 0)
                {
                    var first = waitingTasks.First();
                    runningTask = first.Key;
                    if (!runningTask.HasStarted || runningTask.CanResume)
                    {
                        runningTask.HasStarted = true;
                        runningTaskUpdates = first.Value.Run();
                        runningTaskUpdates
                            .Concat(Observable.Return(BackgroundTaskUpdate.Finished))
                            .Do(update => Messenger.SendMessage<BackgroundTaskUpdate>(update))
                            .Finally(finishTask);
                    }
                    else
                    {
                        first.Value.Cleanup();
                        finishTask();
                    }
                }
            }
            
        }

        public IEnumerable<BackgroundTaskArguments> shutdown()
        {
            lock (this)
            {
                shuttingDown = true;
                if (runningTask != null)
                {
                    var bgtask = waitingTasks[runningTask];
                    bgtask.Cancel();
                    var finalState = bgtask.Arguments;

                    return waitingTasks
                        .Select(kvp => kvp.Key)
                        .Where(args => args != finalState)
                        .Concat(Enumerable.Repeat(finalState,1));
                }
                else
                    return waitingTasks
                       .Select(kvp => kvp.Key);
            }
        }

        public void initialize(IEnumerable<BackgroundTaskArguments> backlog)
        {
            foreach (var task in backlog)
            {
                startTask(task);
            }
        }



        void finishTask()
        {
            lock (this)
            {
                waitingTasks.Remove(runningTask);
                runningTask = null;
                runningTaskUpdates = null;
            }
            if (!shuttingDown) nextTask();
        }
    }
}
