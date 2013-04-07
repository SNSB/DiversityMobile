using DiversityPhone.Model;
using DiversityPhone.Interface;
using System;
using System.ComponentModel;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;

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
                .PipeErrors();               
        }

        public static IObservable<TEventArgs> MakeObservableServiceResultSingle<TEventArgs>(this IObservable<EventPattern<TEventArgs>> This, object userState) where TEventArgs : AsyncCompletedEventArgs
        {
            return MakeObservableServiceResult(This, userState)
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

        public static IObservable<Unit> StoreMapping(this IObservable<int> This, IEntity owner, IKeyMappingService mappingService)
        {
            return This.Do(id => mappingService.AddMapping(owner, id))
                .Select(_ => Unit.Default);
        }

        public static IObservable<WebResponse> DownloadWithCredentials(this IObservable<string> uriObservable, ICurrentCredentials CredentialsProvider)
        {
            return uriObservable
                .SelectMany(uri =>
                    {
                        var creds = CredentialsProvider.CurrentCredentials();
                        var request = WebRequest.CreateHttp(uri);
                        string credentials = Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes(creds.LoginName + ":" + creds.Password));
                        request.Headers["Authorization"] = "Basic " + credentials;

                        return Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.EndGetResponse)();
                    });
        }
    }
}
