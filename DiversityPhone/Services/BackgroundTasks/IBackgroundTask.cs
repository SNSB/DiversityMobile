using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Subjects;
using System.Reactive;

namespace DiversityPhone.Services
{
    public interface IBackgroundTask
    {
        /// <summary>
        /// Sends the argument object used to invoke the Task on completion
        /// </summary>
        IObservable<object> AsyncCompletedNotification { get; }
        /// <summary>
        /// Sends the argument object used to invoke the Task on task failure
        /// </summary>
        IObservable<object> AsyncErrorNotification { get; }
        /// <summary>
        /// Sends the argument object used to invoke the Task on execution
        /// </summary>
        IObservable<object> AsyncStartedNotification { get; }
        /// <summary>
        /// Sends task provided string status messages during execution
        /// </summary>
        IObservable<string> AsyncProgressMessages { get; }
        /// <summary>
        /// Sends boolean values showing wether this Task is currently busy or not
        /// </summary>
        IObservable<bool> BusyObservable { get; }
        /// <summary>
        /// Contains the most recent value transmitted by BusyObservable
        /// </summary>
        bool IsBusy { get; }
        /// <summary>
        /// Contains the argument object used by the current task invocation
        /// </summary>
        object CurrentArguments { get; }
    }
}
