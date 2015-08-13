using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.ServiceModel;

namespace DiversityPhone.Services
{
    internal static class ServiceObservableExtensions
    {
        /// <summary>
        /// Specialization of the <see cref="Observable.Catch(T)"/> Operator that returns an empty observable on error.
        /// </summary>
        public static IObservable<T> CatchEmpty<T>(this IObservable<T> This)
        {
            if (This == null)
            {
                throw new ArgumentNullException("This");
            }

            return This.Catch(Observable.Empty<T>());
        }

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

        public static IObservable<TEventArgs> MakeObservableServiceResult<TEventArgs>(this IObservable<EventPattern<TEventArgs>> This, object userState, IScheduler scheduler) where TEventArgs : AsyncCompletedEventArgs
        {
            return This
                .ObserveOn(scheduler)
                .FilterByUserState(userState)
                .PipeErrors();
        }

        public static IObservable<TEventArgs> MakeObservableServiceResultSingle<TEventArgs>(this IObservable<EventPattern<TEventArgs>> This, object userState, IScheduler scheduler) where TEventArgs : AsyncCompletedEventArgs
        {
            return MakeObservableServiceResult(This, userState, scheduler)
                .ReplayOnlyFirst();
        }

        public static IObservable<T> ConvertToServiceErrors<T>(this IObservable<T> This)
        {
            return This
               .Catch((Exception ex) =>
               {
                   if (ex is FaultException)
                   {
                       return Observable.Throw<T>(new ServiceOperationException(ex.Message, ex));
                   }

                   if (ex is ServerTooBusyException || ex is EndpointNotFoundException || ex is CommunicationException || ex is TimeoutException)
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
            }).ConvertToServiceErrors();
        }

        public static IObservable<Unit> StoreMapping(this IObservable<int> This, IEntity owner, IKeyMappingService mappingService)
        {
            return This.Do(id => mappingService.AddMapping(owner, id))
                .Select(_ => Unit.Default);
        }

        public static IObservable<WebResponse> DownloadWithCredentials(this IObservable<string> uriObservable, ICredentialsService CredentialsProvider)
        {
            return uriObservable
                .Zip(CredentialsProvider.CurrentCredentials(),
                (uri, creds) =>
                {
                    if (creds != null)
                    {
                        var request = WebRequest.CreateHttp(uri);
                        string httpCredentials = Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes(creds.LoginName + ":" + creds.Password));
                        request.Headers["Authorization"] = "Basic " + httpCredentials;

                        return Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.EndGetResponse)()
                            .FirstAsync();
                    }
                    else
                    {
                        return Observable.Empty<WebResponse>();
                    }
                })
                .SelectMany(x => x);
        } 
        public static IObservable<EventPattern<T>> LogErrors<T>(this IObservable<EventPattern<T>> EventStream, IEnableLogger LogOwner) where T : AsyncCompletedEventArgs
        {
            var logger = LogOwner.Log();

            EventStream
                .Subscribe(args =>
                {
                    var error = args.EventArgs.Error;

                    if (error != null)
                    {
                        logger.ErrorException("DiversityService Call Failed", error);
                    }
                });

            return EventStream;
        }
    }
}