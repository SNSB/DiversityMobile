using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Services
{
    public abstract class BackgroundTask
    {
        /// <summary>
        /// Returns the Arguments that were used to create the Task, optionally containing progress in order to allow resuming the Task
        /// Will be called AFTER Cancel
        /// </summary>
        public BackgroundTaskArguments Arguments { get; }
        /// <summary>
        /// Runs the Task on a background Thread, returing an Observable that is used to monitor Progress
        /// </summary>
        /// <returns></returns>
        public IObservable<BackgroundTaskUpdate> Run();

        /// <summary>
        /// Cancels the Task
        /// When this method returns, the cancellation must have been processed
        /// </summary>
        public void Cancel();

        public void Cleanup();

    }
}
