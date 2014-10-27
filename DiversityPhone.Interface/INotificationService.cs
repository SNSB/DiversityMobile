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

            This.showProgress(progressObs.Select(t => new ProgressState(0.0, t)));
        }
    }

    public struct ProgressState
    {
        private static double? PercentToFraction(int? percentage)
        {
            if(!percentage.HasValue)
            {
                return null;
            }
            if(percentage < 0)
            {
                return 0.0;
            }
            if(percentage > 100)
            {
                return 1.0;
            }
            return ((double)percentage) / 100.0;
        }

        public ProgressState(int? percentage, string message)
            : this(PercentToFraction(percentage), message)
        {
        }

        public ProgressState(double? fraction, string message)
        {
            ProgressFraction = fraction;
            ProgressMessage = message;
        }

        public double? ProgressFraction;
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