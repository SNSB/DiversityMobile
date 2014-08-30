using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public static class GenericItemCommands
    {
        public static IReactiveCommand DeleteCommand<T>(
            this IObservable<IElementVM<T>> CurrentElement,
            IMessageBus Messenger,
            INotificationService Notifications,
            IObservable<bool> CanDelete = null)
        {
            var Delete = new ReactiveCommand(CanDelete);

            CurrentElement
                .SampleMostRecent(Delete)
                .SelectMany(toBeDeleted => Notifications.showDecision(DiversityResources.Message_ConfirmDelete)
                    .Where(x => x)
                    .Select(_ => toBeDeleted)
                ).ToMessage(Messenger, MessageContracts.DELETE);

            return Delete;
        }
    }
}