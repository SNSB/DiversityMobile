namespace DiversityPhone.Model
{
    public interface IMultimediaOwner
    {
        ReferrerType OwnerType { get; }
        int OwnerID { get; }
    }
}
