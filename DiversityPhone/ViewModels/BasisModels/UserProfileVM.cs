
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels.BasisModels
{
    public class UserProfileVM : ElementVMBase<UserProfile>
    {        
        public override string Description { get { return Model.ToString(); } }
        public override Icon Icon { get { return ViewModels.Icon.UserProfile; } }
        
        protected override NavigationMessage NavigationMessage
        {
            get { return new NavigationMessage(TargetPage, Model.LoginName); }
        }

        public UserProfileVM(IMessageBus _messenger, UserProfile model, Page targetPage)
            : base(_messenger, model, targetPage)
        {

        }
    }
}
