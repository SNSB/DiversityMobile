namespace DiversityPhone
{
    using DiversityPhone.Interface;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class DispatcherAttribute : Attribute { }

    public class ThreadPoolAttribute : Attribute { }

    internal static class Extensions
    {
        private static readonly TimeSpan NOTIFICATION_DURATION = TimeSpan.FromSeconds(3);

        public static int ListFindIndex<T>(this IList<T> This, Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            for (int idx = 0; idx < This.Count; idx++)
                if (predicate(This[idx]))
                    return idx;

            return -1;
        }

        public static IObservable<bool> BooleanAnd(this IObservable<bool> This, params IObservable<bool>[] parameters)
        {
            return This.CombineLatestMany((a, b) => a && b, parameters);
        }

        public static IObservable<bool> BooleanOr(this IObservable<bool> This, params IObservable<bool>[] parameters)
        {
            return This.CombineLatestMany((a, b) => a || b, parameters);
        }

        private static IObservable<T> CombineLatestMany<T>(this IObservable<T> This, Func<T, T, T> aggregator, params IObservable<T>[] parameters)
        {
            if (This == null)
                throw new ArgumentNullException("This");
            if (aggregator == null)
                throw new ArgumentNullException("aggregator");

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                    if (parameter != null)
                        This = This.CombineLatest(parameter, aggregator);
            }
            return This;
        }

        public static IObservable<T> CastNotNull<T>(this IObservable<object> source) where T : class
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return source
                .Select(x => x as T)
                .Where(x => x != null);
        }

        public static IObservable<T> CheckConnectivity<T>(this IObservable<T> This, IConnectivityService Connectivity, INotificationService Notification)
        {
            return This.SelectMany(val =>
            {
                return
                Connectivity.Status()
                    .FirstAsync()
                    .Where(s =>
                    {
                        if (s != ConnectionStatus.Wifi)
                        {
                            Notification.showNotification(DiversityResources.Info_NoInternet, NOTIFICATION_DURATION);
                            return false;
                        }
                        return true;
                    })
                    .Select(_ => val);
            });
        }

        public static IObservable<T> DisplayProgress<T>(this IObservable<T> This, INotificationService notifications, string text)
        {
            if (This == null)
                throw new ArgumentNullException("This");
            if (notifications == null)
                throw new ArgumentNullException("notifications");
            if (text == null)
                throw new ArgumentNullException("text");

            return Observable.Create((IObserver<T> obs) =>
            {
                var n = notifications.showProgress(text);
                return This.Finally(n.Dispose)
                    .Subscribe(obs);
            });
        }

        public static T NextOrDefault<T>(this IEnumerator<T> This)
        {
            Contract.Requires(This != null);

            if (This.MoveNext())
            {
                return This.Current;
            }
            else
            {
                return default(T);
            }
        }

        public static Task<T> ToTask<T>(this IObservable<T> This)
        {
            return ToTask(This, CancellationToken.None);
        }

        public static Task<T> ToTask<T>(this IObservable<T> This, CancellationToken cancellationToken)
        {
            Contract.Requires(This != null);

            var completion = new TaskCompletionSource<T>();

            var subscription = This
                .LastAsync()
                .Subscribe(
                    (x) => completion.TrySetResult(x),
                    (err) => completion.TrySetException(err),
                    () => { }
                );

            cancellationToken.Register(subscription.Dispose, useSynchronizationContext: false);

            return completion.Task;
        }
    }
}