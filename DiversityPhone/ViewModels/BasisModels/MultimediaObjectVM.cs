
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

        //General Implementation
        public override Icon Icon
        {
            get
            {
                switch (Model.MediaType)
                {
                    case MediaType.Image:
                        return Icon.Photo;
                    case MediaType.Audio:
                        return Icon.Audio;
                    case MediaType.Video:
                        return Icon.Video;
                    default:
                        return Icon.Photo;
                }
            }
        }


        //Implementation as String for beeing able to display thumbs
        public string IconPath
        {
            get
            {
                switch(Model.MediaType)
                {
                    case MediaType.Image:
                        return "/Images/appbar.feature.camera.rest.png";
                    case MediaType.Audio:
                            return "/Images/appbar.feature.audio.rest.png";
                    case MediaType.Video:
                            return "/Images/appbar.feature.video.rest.png";
                    default:
                            return "/Images/appbar.feature.camera.rest.png";
                 }
            }
        }       

        public MultimediaObjectVM( MultimediaObject model)
            : base(model)
        {

        }
    }
}
