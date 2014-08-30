namespace DiversityPhone.Services
{
    using DiversityPhone.Interface;
    using ReactiveUI;
    using System.Reactive.Linq;
    using System.Threading.Tasks;


    public class CleanupService : ICleanupData
    {
        public async Task ClearLocalData()
        {
            var newProfile = Profile.CreateProfileID();
            Profile.SetCurrentProfileID(newProfile);
            await Cloud.DisconnectAsync();
            await ClearBackups();
        }

        private async Task ClearBackups()
        {
            var snapshots = await Backup.EnumerateSnapshots().ToList();
            foreach (var snap in snapshots)
            {
                await Backup.DeleteSnapshot(snap.FolderPath);
            }
        }

        private readonly ICurrentProfile Profile;
        private readonly CloudStorageService Cloud;
        private readonly IBackupService Backup;

        public CleanupService(
            ICurrentProfile Profile,
            IBackupService Backup,
            CloudStorageService Cloud
            )
        {
            this.Profile = Profile;
            this.Backup = Backup;
            this.Cloud = Cloud;
        }
    }
}
