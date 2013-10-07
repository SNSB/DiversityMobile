
namespace DiversityPhone.Model
{
    public interface IChildEntity
    {
        DBObjectType ParentType { get; }
        int? ParentID { get; }
    }
}
