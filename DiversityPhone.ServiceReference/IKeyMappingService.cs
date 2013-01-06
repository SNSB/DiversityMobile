using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Services
{

    public interface IKeyMappingService
    {
        int? ResolveKey(DBObjectType ownerType, int ownerID);
        void AddMapping(DBObjectType ownerType, int ownerID, int serverID);

        int? ResolveKey<T>(T entity);
    }

    public static class MappingExtensions
    {
        public static int? ResolveKey(this IKeyMappingService mapping, IOwner owner)
        {
            return mapping.ResolveKey(owner.OwnerType, owner.OwnerID);
        }

        public static int EnsureKey(this IKeyMappingService mapping, DBObjectType ownerType, int ownerID)
        {
            var key = mapping.ResolveKey(ownerType, ownerID);
            if (!key.HasValue)
                throw new KeyNotFoundException("no Mapping found");
            else
                return key.Value;
        }

        public static void AddMapping(this IKeyMappingService mapping, IOwner owner, int serverID)
        {
            mapping.AddMapping(owner.OwnerType, owner.OwnerID, serverID);
        }


    }
}
