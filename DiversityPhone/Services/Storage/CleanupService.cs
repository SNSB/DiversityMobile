namespace DiversityPhone.Services {
    using DiversityPhone.Interface;



    public class CleanupService : ICleanupData {
        public void ClearLocalData() {
            var newProfile = Profile.CreateProfileID();
            Profile.SetCurrentProfileID(newProfile);
            Cloud.Disconnect();
        }

        private readonly ICurrentProfile Profile;
        private readonly CloudStorageService Cloud;

        public CleanupService(
            ICurrentProfile Profile,
            CloudStorageService Cloud
            ) {
            this.Profile = Profile;
            this.Cloud = Cloud;
        }
    }
}
