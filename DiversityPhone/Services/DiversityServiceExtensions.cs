using System;
using ReactiveUI;
using System.Reactive.Linq;
using System.ServiceModel;
namespace DiversityPhone.Services
{
    public static class DiversityServiceExtensions
    {
        public static IObservable<T> OnServiceUnavailable<T>(this IObservable<T> This, Func<T> onException)
        {
            var messenger = App.IOC.Resolve<IMessageBus>();

            return This
                .Catch((Exception ex) =>
                {
                    if (ex is ServerTooBusyException || ex is EndpointNotFoundException || ex is CommunicationException)
                    {
                        if (onException != null)
                            return Observable.Return(onException());
                        else
                            return Observable.Return(default(T));
                    }
                    else
                        throw (ex);                    
                });
                
        }
    }
}