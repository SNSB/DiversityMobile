using System;
using System.Diagnostics.Contracts;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

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
            Contract.Requires(This != null);

            showNotification(This, text, DEFAULT_NOTIFICATION_DURATION);
        }

        public static void showNotification(this INotificationService This, string text, TimeSpan duration)
        {
            Contract.Requires(This != null);

            var progressObs = Observable.Return(text)
                .Concat(Observable.Never(text))
                .Timeout(duration, Observable.Empty(text), This.NotificationScheduler);

            This.showProgress(progressObs.Select(t => new ProgressState(0, t)));
        }
    }

    public struct ProgressState
    {
        public ProgressState(int? percent, string message)
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

        IObservable<Unit> showPopup(string text);

        IObservable<bool> showDecision(string text);
    }
}