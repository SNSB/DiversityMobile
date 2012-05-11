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
        IObservable<object> AsyncCompletedNotification { get; }
        IObservable<object> AsyncCleanupNotification { get; }
        IObservable<object> AsyncStartedNotification { get; }
        IObservable<string> AsyncProgressMessages { get; }
        IObservable<int> ItemsInflight { get; }
        int CurrentItemsInFlight { get; }
        object CurrentArguments { get; }
    }
}
