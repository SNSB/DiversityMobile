using DiversityPhone.Model;
using DiversityPhone.Interface;
using System;
using System.ComponentModel;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.ServiceModel;
using System.Linq;

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

        public static IObservable<T> ConvertErrors<T>(this IObservable<T> This)
        {
            return This
               .Catch((Exception ex) =>
               {
                   if (ex is FaultException)
                   {
                       return Observable.Throw<T>(new ServiceOperationException(ex.Message, ex));
                   }

                   if (ex is ServerTooBusyException || ex is EndpointNotFoundException || ex is CommunicationException)
                   {
                       return Observable.Throw<T>(new ServiceNotAvailableException(ex.Message, ex));
                   }
                  
                   return Observable.Throw<T>(ex);
               });
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
            }).ConvertErrors();
        }

        public static IObservable<Unit> StoreMapping(this IObservable<int> This, IEntity owner, IKeyMappingService mappingService)
        {
            return This.Do(id => mappingService.AddMapping(owner, id))
                .Select(_ => Unit.Default);
        }

        public static IObservable<WebResponse> DownloadWithCredentials(this IObservable<string> uriObservable, ICredentialsService CredentialsProvider)
        {
            var mostRecentUserCreds = CredentialsProvider.CurrentCredentials().MostRecent(null);

            return uriObservable
                .SelectMany(uri =>
                    {
                        var creds = mostRecentUserCreds.First();
                        if (creds != null)
                        {
                            var request = WebRequest.CreateHttp(uri);
                            string httpCredentials = Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes(creds.LoginName + ":" + creds.Password));
                            request.Headers["Authorization"] = "Basic " + httpCredentials;

                            return Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.EndGetResponse)();
                        }
                        else
                        {
                            return Observable.Empty<WebResponse>();
                        }
                    });
        }
    }
}
