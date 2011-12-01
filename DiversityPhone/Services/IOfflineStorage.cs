using DiversityPhone.Model;
using System.Collections.Generic;
namespace DiversityPhone.Services
{
    public interface IOfflineStorage : IOfflineFieldData, IOfflineVocabulary
    {
        IList<UserProfile> getAllUserProfiles();
        UserProfile getUserProfile(string loginName);



       
    }
}
