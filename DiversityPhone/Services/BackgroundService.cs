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
        Dictionary<BackgroundTaskArguments, IBackgroundTask> waitingTasks = new Dictionary<BackgroundTaskArguments, IBackgroundTask>();
        BackgroundTaskArguments runningTask;
        IObservable<BackgroundTaskUpdate> runningTaskUpdates;

        public BackgroundService(Container ioc, IEnumerable<BackgroundTaskArguments> backlog)
        {
            Messenger = ioc.Resolve<IMessageBus>();
            TaskFactory = ioc.Resolve<IBackgroundTaskFactory>();

            Messenger.Listen<BackgroundTaskArguments>(MessageContracts.BACKGROUNDPROCESS_START)
                .StartWith(backlog.ToArray())
                .Subscribe(proc => startProcess(proc));

        }

        public void startProcess(BackgroundTaskArguments args)
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
                    runningTaskUpdates = first.Value.Run(); 
                    runningTaskUpdates
                        .Concat(Observable.Return(BackgroundTaskUpdate.Finished))
                        .Do(update => Messenger.SendMessage<BackgroundTaskUpdate>(update))
                        .Finally(() =>
                                {
                                    lock (this)
                                    {
                                        runningTask = null;
                                        runningTaskUpdates = null;
                                    }
                                    if (!shuttingDown) nextTask();
                                });
                }
            }
            
        }

        IEnumerable<BackgroundTaskArguments> shutdown()
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

        
    }
}
