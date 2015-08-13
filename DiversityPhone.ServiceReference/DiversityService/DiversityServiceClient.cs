using DiversityPhone.DiversityService;
using DiversityPhone.Interface;
using DiversityPhone.Model;
using DiversityPhone.PhoneMediaService;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Client = DiversityPhone.Model;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient : IDiversityServiceClient, IEnableLogger
    {
        private readonly TimeSpan SERVICE_CALL_TIMEOUT = TimeSpan.FromMinutes(1);

        List<object> CallsInFlight = new List<object>();
        private void PushCall(object svc)
        {
            lock (CallsInFlight)
            {
                CallsInFlight.Add(svc);
            }
        }

        private void PopCall(object svc)
        {
            lock(CallsInFlight)
            {
                CallsInFlight.Remove(svc);
            }
        }
        private IObservable<TResult> DiversityServiceCall<TArgs, TResult>(Func<TArgs, TResult> map, Action<DiversityService.DiversityServiceClient> inner) where TArgs : AsyncCompletedEventArgs
        {
            return DiversityServiceCallObservable<TArgs, TResult>(obs =>
                obs.Select(map),
                inner);
        }
        private IObservable<TResult> DiversityServiceCallObservable<TArgs, TResult>(Func<IObservable<TArgs>, IObservable<TResult>> map, Action<DiversityService.DiversityServiceClient> inner) where TArgs : AsyncCompletedEventArgs
        {
            var eventName = typeof(TArgs).Name.Replace("EventArgs", "");

            // Allows retrying a service call by resubscribing
            return Observable.Create<TResult>(observer =>
            {
                var svc = new DiversityService.DiversityServiceClient();

                PushCall(svc);

                var res1 = Observable.FromEventPattern<TArgs>(svc, eventName, ThreadPool)
                    .Take(1)
                    .Timeout(SERVICE_CALL_TIMEOUT)
                    .Finally(() => PopCall(svc))
                    .LogErrors(this)
                    .Select(p => p.EventArgs)
                    .PipeErrors();

                var res2 = map(res1);

                var sub = res2.Subscribe(observer);

                inner(svc);

                return sub;
            });
        }

        private PhoneMediaService.PhoneMediaServiceClient _multimedia = new PhoneMediaService.PhoneMediaServiceClient();

        //MULTIMEDIA
        private IObservable<EventPattern<PhoneMediaService.SubmitCompletedEventArgs>> UploadMultimediaCompleted;

        private IObservable<EventPattern<PhoneMediaService.BeginTransactionCompletedEventArgs>> BeginTransactionCompleted;
        private IObservable<EventPattern<PhoneMediaService.EncodeFileCompletedEventArgs>> EncodeFileCompleted;
        private IObservable<EventPattern<PhoneMediaService.CommitCompletedEventArgs>> CommitCompleted;
        private IObservable<EventPattern<AsyncCompletedEventArgs>> RollbackCompleted;

        private readonly IKeyMappingService Mapping;
        private readonly ICredentialsService Credentials;
        private readonly IScheduler ThreadPool;

        public DiversityServiceClient(ICredentialsService Credentials, IKeyMappingService Mapping, [ThreadPool] IScheduler ThreadPool)
        {
            this.Mapping = Mapping;
            this.Credentials = Credentials;
            this.ThreadPool = ThreadPool;

            UploadMultimediaCompleted = Observable.FromEventPattern<SubmitCompletedEventArgs>(h => _multimedia.SubmitCompleted += h, h => _multimedia.SubmitCompleted -= h, ThreadPool);
            LogErrors<SubmitCompletedEventArgs>(UploadMultimediaCompleted);
            BeginTransactionCompleted = Observable.FromEventPattern<BeginTransactionCompletedEventArgs>(h => _multimedia.BeginTransactionCompleted += h, h => _multimedia.BeginTransactionCompleted -= h, ThreadPool);
            LogErrors<BeginTransactionCompletedEventArgs>(BeginTransactionCompleted);
            EncodeFileCompleted = Observable.FromEventPattern<EncodeFileCompletedEventArgs>(h => _multimedia.EncodeFileCompleted += h, h => _multimedia.EncodeFileCompleted -= h, ThreadPool);
            LogErrors<EncodeFileCompletedEventArgs>(EncodeFileCompleted);
            CommitCompleted = Observable.FromEventPattern<CommitCompletedEventArgs>(h => _multimedia.CommitCompleted += h, h => _multimedia.CommitCompleted -= h, ThreadPool);
            LogErrors<CommitCompletedEventArgs>(CommitCompleted);
            RollbackCompleted = Observable.FromEventPattern<AsyncCompletedEventArgs>(h => _multimedia.RollbackCompleted += h, h => _multimedia.RollbackCompleted -= h, ThreadPool);
        }

        private void WithCredentials(Action<UserCredentials> action)
        {
            Credentials.CurrentCredentials()
                .Where(c => c != null)
                .FirstAsync()
                .Subscribe(action);
        }

        private void LogErrors<T>(IObservable<EventPattern<T>> EventStream) where T : AsyncCompletedEventArgs
        {
            var logger = this.Log();

            EventStream
                .Subscribe(args =>
                {
                    var error = args.EventArgs.Error;

                    if (error != null)
                    {
                        logger.ErrorException("DiversityService Call Failed", error);
                    }
                });
        }
    }
}