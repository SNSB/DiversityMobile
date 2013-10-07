
namespace DiversityPhone.Model
{
    public enum ModificationState
    {
        New, // Persisted Nowhere
        Unmodified, // Persisted Remotely & Locally
        Modified // Persisted Locally
    }
    public interface IModifyable
    {
        /// <summary>
        /// Tracks the persistance status of an object.
        /// </summary>
        ModificationState ModificationState { get; set; }
    }

    public static class ModifyableMixin
    {
        public static bool IsNew(this IModifyable modObj)
        {
            return modObj.ModificationState == ModificationState.New;
        }
        public static bool IsModified(this IModifyable modObj)
        {
            return modObj.ModificationState == ModificationState.Modified;
        }
        public static bool IsUnmodified(this IModifyable modObj)
        {
            return modObj.ModificationState == ModificationState.Unmodified;
        }

    }
}
