using System;
using ReactiveUI;
using System.Reactive.Linq;
using System.ServiceModel;
namespace DiversityPhone.Services
{
    public static class DiversityServiceExtensions
    {
        public static IObservable<T> handleServiceExceptions<T>(this IObservable<T> This, T def = default(T))
        {
            var messenger = App.IOC.Resolve<IMessageBus>();

            return This
                .Catch((Exception ex) =>
                {
                    if (ex is ServerTooBusyException || ex is EndpointNotFoundException)
                        messenger.SendMessage(
                            new DialogMessage(Messages.DialogType.OK,
                                DiversityResources.Setup_Message_SorryHeader,
                                DiversityResources.Setup_Message_ServiceUnavailable_Body));

                    return Observable.Return<T>(def);
                });
                
        }
    }
}