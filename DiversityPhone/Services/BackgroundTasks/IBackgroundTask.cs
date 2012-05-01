﻿using System;
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
        IObservable<Exception> AsyncErrorNotification { get; }
        IObservable<object> AsyncStartedNotification { get; }
        IObservable<bool> CanExecuteObservable { get;  }
        IObservable<int> ItemsInflight { get; }
        object CurrentArguments { get; }
    }
}
