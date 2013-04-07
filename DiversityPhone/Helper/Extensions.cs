using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.ServiceModel;
using ReactiveUI;
using DiversityPhone.Interface;
using DiversityPhone.Model;
using Ninject.Modules;
using Ninject.Activation;
using Ninject.Parameters;
using Ninject;
using System.Linq;

namespace DiversityPhone.ViewModels
{
    public class DispatcherAttribute : Attribute { }
    public class ThreadPoolAttribute : Attribute { }

    public class FuncModule : NinjectModule
    {
        public override void Load()
        {
            this.Kernel.Bind(typeof(Func<>)).ToMethod(CreateFunc).When(VerifyFactoryFunction);
        }

        private static bool VerifyFactoryFunction(IRequest request)
        {
            var genericArguments = request.Service.GetGenericArguments();
            if (genericArguments.Count() != 1)
            {
                return false;
            }

            var instanceType = genericArguments.Single();
            return request.ParentContext.Kernel.CanResolve(new Request(genericArguments[0], null, new IParameter[0], null, false, true)) ||
                   TypeIsSelfBindable(instanceType);
        }

        private static object CreateFunc(IContext ctx)
        {
            var functionFactoryType = typeof(FunctionFactory<>).MakeGenericType(ctx.GenericArguments);
            var ctor = functionFactoryType.GetConstructors().Single();
            var functionFactory = ctor.Invoke(new object[] { ctx.Kernel });
            return functionFactoryType.GetMethod("Create").Invoke(functionFactory, new object[0]);
        }

        private static bool TypeIsSelfBindable(Type service)
        {
            return !service.IsInterface
                   && !service.IsAbstract
                   && !service.IsValueType
                   && service != typeof(string)
                   && !service.ContainsGenericParameters;
        }

        public class FunctionFactory<T>
        {
            private readonly IKernel kernel;

            public FunctionFactory(IKernel kernel)
            {
                this.kernel = kernel;
            }

            public Func<T> Create()
            {
                return () => this.kernel.Get<T>();
            }
        }
    }

    static class Extensions
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
                                if(s != ConnectionStatus.Wifi) 
                                {
                                    Notification.showNotification(DiversityResources.Info_NoInternet, NOTIFICATION_DURATION );
                                    return false;
                                }
                                return true;
                            })
                        .Select(_ => val);
                });
        }

        public static IObservable<T> HandleServiceErrors<T>(this IObservable<T> This, INotificationService Notification, IMessageBus Messenger, IObservable<T> ErrorValue = null)
        {
            return This
                .Catch((Exception ex) =>
                {
                    var rethrow = ErrorValue == null;
                    var handled = false;
                    if (ex is ServerTooBusyException || ex is EndpointNotFoundException || ex is CommunicationException)
                    {
                        Notification.showNotification(DiversityResources.Info_ServiceUnavailable, NOTIFICATION_DURATION);
                        handled = true;
                    }
                    if (ex is FaultException)
                    {
                        Messenger.SendMessage(new DialogMessage(DialogType.OK, DiversityResources.Message_SorryHeader, DiversityResources.Message_ServiceProblem + ex.Message));
                        handled = true;
                    }

                    if (!handled || rethrow)
                        return Observable.Throw<T>(ex);
                    else
                        return ErrorValue;
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
    }
}
