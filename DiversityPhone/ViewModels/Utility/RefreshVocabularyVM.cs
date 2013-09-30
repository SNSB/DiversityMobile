using DiversityPhone.Interface;
using DiversityPhone.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using DiversityPhone.Model;

namespace DiversityPhone.ViewModels
{
    public class RefreshVocabularyVM : PageVMBase
    {
        public RefreshVocabularyVM(
            ICredentialsService Credentials,
            Func<IRefreshVocabularyTask> refreshVocabluaryTaskFactory
            )
        {
            this.OnActivation()
                .SelectMany(_ => Credentials.CurrentCredentials().Where(cred => cred != null))
                .Subscribe(login =>
                    {
                        var refreshTask = refreshVocabluaryTaskFactory();
                        refreshTask.Start(login)
                            .Subscribe(_ => {}, () =>
                            {
                                Messenger.SendMessage<EventMessage>(EventMessage.Default, MessageContracts.REFRESH);
                                Messenger.SendMessage<Page>(Page.Home);
                            });
                    });
                
        }

    }
}
