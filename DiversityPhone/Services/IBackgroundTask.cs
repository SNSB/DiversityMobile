using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Services
{
    public interface IBackgroundTask
    {
        /// <summary>
        /// Returns the Arguments that were used to create the Task, optionally containing progress in order to allow resuming the Task
        /// Will be called AFTER Cancel
        /// </summary>
        BackgroundTaskArguments Arguments { get; }
        /// <summary>
        /// Runs the Task on a background Thread, returing an Observable that is used to monitor Progress
        /// </summary>
        /// <returns></returns>
        IObservable<BackgroundTaskUpdate> Run();

        /// <summary>
        /// Cancels the Task
        /// When this method returns, the cancellation must have been processed
        /// </summary>
        void Cancel();

    }
}
