using DiversityPhone.Model;
using DiversityPhone.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

namespace DiversityPhone.Services
{
    internal static class ServiceObservableExtensions
    {
        public static IObservable<TEventArgs> FilterByUserState<TEventArgs>(this IObservable<EventPattern<TEventArgs>> This, object userState) where TEventArgs : AsyncCompletedEventArgs
        {
            return This.Where(p => p.EventArgs.UserState == userState).Select(p => p.EventArgs);
        }

        public static IObservable<T> ReplayOnlyFirst<T>(this IObservable<T> This)
        {
            var res = This.Take(1).Replay(1);
            res.Connect();
            return res;
        }


        public static IObservable<TEventArgs> MakeObservableServiceResult<TEventArgs>(this IObservable<EventPattern<TEventArgs>> This, object userState) where TEventArgs : AsyncCompletedEventArgs
        {
            return This.FilterByUserState(userState)
                .PipeErrors()
                .ReplayOnlyFirst();
        }

        public static IObservable<TEventArgs> PipeErrors<TEventArgs>(this IObservable<TEventArgs> This) where TEventArgs : AsyncCompletedEventArgs
        {
            return Observable.Create<TEventArgs>(obs =>
            {
                if (obs == null)
                    throw new ArgumentNullException("obs");

                IDisposable subscription = null;
                try
                {
                    subscription = This.Subscribe(
                        p =>
                        {
                            if (p.Error != null)
                                obs.OnError(p.Error);
                            else if (p.Cancelled)
                                obs.OnCompleted();
                            else
                                obs.OnNext(p);
                        },
                        obs.OnError,
                        obs.OnCompleted);
                }
                catch (Exception ex)
                {
                    obs.OnError(ex);
                    if (subscription != null)
                        subscription.Dispose();
                }
                return subscription;
            });
        }

        public static IObservable<Unit> StoreMapping(this IObservable<int> This, IOwner owner, IKeyMappingService mappingService)
        {
            return This.Do(id => mappingService.AddMapping(owner, id))
                .Select(_ => Unit.Default);
        }
    }
}
