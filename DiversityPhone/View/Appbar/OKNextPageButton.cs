using DiversityPhone.Model;
using Microsoft.Phone.Shell;
using ReactiveUI;
using System;
using System.Diagnostics.Contracts;

namespace DiversityPhone.View.Appbar
{
    public class OKNextPageButton : ApplicationBarIconButton
    {
        private IMessageBus Messenger;
        private Page TargetPage;


        public OKNextPageButton(IMessageBus Messenger, Page TargetPage, IObservable<bool> CanNavigate = null)
            : base(new Uri("/Images/appbar.check.rest.png", UriKind.Relative))
        {
            Contract.Requires(Messenger != null);

            this.Messenger = Messenger;
            this.TargetPage = TargetPage;

            if (CanNavigate != null)
            {
                CanNavigate
                    .Subscribe(can => this.IsEnabled = can);
            }

            this.Click += OKNextPageButton_Click;

            this.Text = DiversityResources.Button_OK;
        }

        void OKNextPageButton_Click(object sender, EventArgs e)
        {
            Messenger.SendMessage(TargetPage);
        }

    }
}
