
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;


namespace DiversityPhone.ViewModels
{
    public class MultimediaObjectVM : ElementVMBase<MultimediaObject>
    {        
        public override string Description { get { return Model.MediaType.ToString(); } }
        public override Icon Icon
        {
            get
            {
                switch (Model.MediaType)
                {
                    case Services.MediaType.Audio:
                        return ViewModels.Icon.Audio;
                    case Services.MediaType.Video:
                        return ViewModels.Icon.Video;
                    case Services.MediaType.Image:
                        return ViewModels.Icon.Photo;
                    default:
                        return ViewModels.Icon.Photo;
                }
            }
        }

        protected override NavigationMessage NavigationMessage
        {
            get { return new NavigationMessage(TargetPage, Model.Uri); }
        }

        public MultimediaObjectVM(IMessageBus _messenger, MultimediaObject model, Page targetPage)
            : base(_messenger, model, targetPage)
        {

        }
    }
}
