using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;


namespace DiversityPhone.ViewModels
{
    public class MultimediaObjectVM : ElementVMBase<MultimediaObject>
    {        
        public override string Description { get { return Model.ToString(); } }
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

        public MultimediaObjectVM(IMessageBus messenger,MultimediaObject model)
         : base(messenger, model)
        {
            
        }
    }
}
