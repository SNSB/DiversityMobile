namespace DiversityPhone.Model
{
    public interface IOwner
    {
        DBObjectType OwnerType { get; }
        int OwnerID { get; }
    }

    public interface IMultimediaOwner : IOwner
    {        
    }

    public interface ILocationOwner : IOwner
    {
    }
}
