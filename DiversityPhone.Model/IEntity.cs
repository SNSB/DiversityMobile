namespace DiversityPhone.Model
{
    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class EntityKeyAttribute : System.Attribute
    {
    }

    public interface IEntity
    {
        DBObjectType EntityType { get; }

        int EntityID { get; }
    }

    public interface IMultimediaOwner : IEntity
    {
    }

    public interface ILocationOwner : IEntity
    {
    }
}