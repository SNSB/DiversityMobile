using DiversityPhone.Model;

namespace DiversityPhone.Services
{
    public interface IMultimediaOwner
    {
        ReferrerType OwnerType { get; }
        int OwnerID { get; }
    }
}
