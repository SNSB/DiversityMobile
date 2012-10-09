namespace DiversityPhone.Model
{
    public interface IOwner
    {
        ReferrerType OwnerType { get; }
        int OwnerID { get; }
    }

    public interface IMultimediaOwner : IOwner
    {        
    }

    public interface ILocationOwner : IOwner
    {
    }
}
