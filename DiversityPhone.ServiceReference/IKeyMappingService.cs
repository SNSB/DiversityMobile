using DiversityPhone.Model;
using System.Collections.Generic;

namespace DiversityPhone.Interface
{
    public interface IKeyMappingService
    {
        int? ResolveToServerKey(DBObjectType ownerType, int ownerID);

        int? ResolveToLocalKey(DBObjectType ownerType, int ownerID);

        void AddMapping(DBObjectType ownerType, int ownerID, int serverID);
    }

    public static class MappingExtensions
    {
        public static int? ResolveKey(this IKeyMappingService mapping, IEntity owner)
        {
            return mapping.ResolveToServerKey(owner.EntityType, owner.EntityID);
        }

        public static int EnsureKey(this IKeyMappingService mapping, DBObjectType ownerType, int ownerID)
        {
            var key = mapping.ResolveToServerKey(ownerType, ownerID);
            if (!key.HasValue)
                throw new KeyNotFoundException(string.Format("no Mapping for type {0}, id {1} found", ownerType, ownerID));
            else
                return key.Value;
        }

        public static void AddMapping(this IKeyMappingService mapping, IEntity owner, int serverID)
        {
            mapping.AddMapping(owner.EntityType, owner.EntityID, serverID);
        }
    }
}