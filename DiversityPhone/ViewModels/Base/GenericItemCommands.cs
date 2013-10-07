using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DiversityPhone.ViewModels
{
    public static class GenericItemCommands
    {
        public static IReactiveCommand DeleteCommand<T>(this IObservable<IElementVM<T>> CurrentElement, IMessageBus Messenger, IObservable<bool> CanDelete = null)
        {
            var Delete = new ReactiveCommand(CanDelete);
            var ConfirmedDelete = new Subject<IElementVM<T>>();

            CurrentElement
                .SampleMostRecent(Delete)
                .Select(toBeDeleted => new DialogMessage(DialogType.YesNo, "", DiversityResources.Message_ConfirmDelete, 
                    (res) => { 
                        if (res == DialogResult.OKYes) 
                            ConfirmedDelete.OnNext(toBeDeleted);
                    }))
                .Subscribe(d => Messenger.SendMessage(d));

            ConfirmedDelete
                .Subscribe(tbd => Messenger.SendMessage(tbd, MessageContracts.DELETE));

            return Delete;
        }

    }
}
