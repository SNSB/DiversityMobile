using DiversityPhone.Model;
using ReactiveUI;

namespace DiversityPhone.ViewModels
{
    public class MultimediaObjectVM : ReactiveObject, IElementVM<MultimediaObject>
    {
        public MultimediaObject Model
        {
            get;
            private set;
        }

        public string Description { get { return Model.MediaType.ToString(); } }

        public Icon Icon
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

        object IElementVM.Model
        {
            get { return Model; }
        }

        public MultimediaObjectVM(MultimediaObject model)
        {
            Model = model;
        }
    }
}