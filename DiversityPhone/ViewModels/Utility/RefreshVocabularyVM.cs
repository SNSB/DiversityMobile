namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using DiversityPhone.Services;
    using ReactiveUI;
    using System;
    using System.Reactive.Linq;

    public class RefreshVocabularyVM : PageVMBase
    {
        public RefreshVocabularyVM(
            Func<IRefreshVocabularyTask> refreshVocabularyTaskFactory,
            IMessageBus Messenger,
            INotificationService Notifications
            )
        {
            Action navigateHome = () =>
            {
                Messenger.SendMessage<EventMessage>(EventMessage.Default, MessageContracts.INIT);
                Messenger.SendMessage<Page>(Page.Home);
            };
            this.OnActivation()
                .Select(_ => refreshVocabularyTaskFactory())
                .Select(task =>
                    task.Start()
                    .ShowServiceErrorNotifications(Notifications)
                    .Subscribe(_ => { }, _ => navigateHome(), navigateHome)
                ).Subscribe();
        }

    }
}