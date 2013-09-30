using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace DiversityPhone.Interface
{
    public static class NotificationMixin
    {
        private static TimeSpan DEFAULT_NOTIFICATION_DURATION = TimeSpan.FromSeconds(2);

        public static IDisposable showProgress(this INotificationService This, string text)
        {
            Contract.Requires(This != null);

            var textSubject = new Subject<string>();

            This.showProgress(textSubject);

            textSubject.OnNext(text);

            return Disposable.Create(textSubject.OnCompleted);                
        }

        public static void showProgress(this INotificationService This, IObservable<string> progressTexts)
        {
            Contract.Requires(This != null);

            This.showProgress(progressTexts.Select(t => new ProgressState(null, t)));
        }

        public static void showNotification(this INotificationService This, string text)
        {
            showNotification(This, text, DEFAULT_NOTIFICATION_DURATION);
        }

        public static void showNotification(this INotificationService This, string text, TimeSpan duration)
        {
            Contract.Requires(This != null);
            
            var progressObs = Observable.Return(text)
                .Concat(Observable.Never(text))
                .TakeUntil(This.NotificationScheduler.Now.Add(duration), This.NotificationScheduler);

            This.showProgress(progressObs);
        }
    }

    public struct ProgressState
    {
        public ProgressState (int? percent, string message)
	    {
            ProgressPercentage = percent;
            ProgressMessage = message;
	    }

        public int? ProgressPercentage;
        public string ProgressMessage;
    }

    public interface INotificationService
    {
        IScheduler NotificationScheduler { get; }
        void showProgress(IObservable<ProgressState> progress);
    }
}
