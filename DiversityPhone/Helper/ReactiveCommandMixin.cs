namespace DiversityPhone
{
    using DiversityPhone.Interface;
    using ReactiveUI.Xaml;
    using System;
    using System.Diagnostics.Contracts;
    using System.Reactive.Linq;

    public static class ReactiveCommandMixin
    {
        public static void ShowInFlightNotification(this IReactiveAsyncCommand This, INotificationService Notifications, string Notification)
        {
            Contract.Requires(This != null);
            Contract.Requires(Notifications != null);

            This.AsyncStartedNotification
                .Select(_ => new ProgressState() { ProgressMessage = Notification, ProgressFraction = null })
                .Select(n => ObservableMixin.ReturnAndNever(n)
                    .TakeUntil(This.AsyncCompletedNotification))
                .Subscribe(Notifications.showProgress);
        }

        public static IObservable<T> ShowServiceErrorNotifications<T>(this IObservable<T> This, INotificationService Notifications)
        {
            Contract.Requires(This != null);
            Contract.Requires(Notifications != null);

            return This
               .Catch<T, ServiceNotAvailableException>(ex =>
               {
                   Notifications.showNotification(DiversityResources.Info_ServiceUnavailable);
                   return Observable.Empty<T>();
               })
               .Catch<T, ServiceOperationException>(ex =>
               {
                   Notifications.showPopup(DiversityResources.Message_ServiceProblem + ex.Message);
                   return Observable.Empty<T>();
               });
        }

        public static IObservable<T> ShowErrorNotifications<T>(this IObservable<T> This, INotificationService Notifications)
        {
            Contract.Requires(This != null);
            Contract.Requires(Notifications != null);

            return This
               .Catch<T, Exception>(ex =>
               {
                   Notifications.showNotification(DiversityResources.Info_ErrorOcurred);
                   return Observable.Empty<T>();
               });
        }

        public static IObservable<bool> AndNoItemsInFlight(this IObservable<bool> This, IReactiveAsyncCommand Command)
        {
            Contract.Requires(This != null);
            Contract.Requires(Command != null);

            return This.CombineLatest(
                    Command.ItemsInflight.Select(x => x == 0),
                    (t, command) => t & command);
        }
    }
}