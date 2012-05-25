using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Model
{
    public enum ModificationState
    {
        New,
        Unmodified,
        Modified
    }
    public interface IModifyable
    {
        /// <summary>
        /// Tracks the persistance status of an object.
        /// null - Newly created, not yet persisted
        /// false - persisted remotely, local copy unchanged
        /// true - persisted only locally OR the local copy has been changed
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
