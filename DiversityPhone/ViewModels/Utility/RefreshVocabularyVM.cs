using DiversityPhone.Interface;
using DiversityPhone.Model;
using DiversityPhone.Services;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels {
    public class RefreshVocabularyVM : PageVMBase {
        public RefreshVocabularyVM(
            ICredentialsService Credentials,
            Func<IRefreshVocabularyTask> refreshVocabluaryTaskFactory,
            IMessageBus Messenger
            ) {
            this.OnActivation()
                .SelectMany(_ => Credentials.CurrentCredentials().Where(cred => cred != null).FirstAsync())
                .TakeUntil(this.OnDeactivation())
                .Subscribe(login => {
                    var refreshTask = refreshVocabluaryTaskFactory();
                    refreshTask.Start(login)
                        .Subscribe(_ => { }, () => {
                            Messenger.SendMessage<EventMessage>(EventMessage.Default, MessageContracts.INIT);
                            Messenger.SendMessage<Page>(Page.Home);
                        });
                });

        }

    }
}
