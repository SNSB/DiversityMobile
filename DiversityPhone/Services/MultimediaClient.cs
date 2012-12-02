using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DiversityPhone.Model;
using System.Reactive.Linq;
using DiversityPhone.MultimediaService;
using System.IO.IsolatedStorage;
using System.IO;
using DiversityPhone.DiversityService;
using Client = DiversityPhone.Model;

namespace DiversityPhone.Services
{
    public interface IMultiMediaClient
    {
        IObservable<String> UploadMultiMediaObjectRawData(Client.MultimediaObject mmo);
    }


    public class MultimediaClient : IMultiMediaClient
    {
        private MediaService4Client _msc = new MediaService4Client();
        private ISettingsService _settings;
        
        public MultimediaClient(ISettingsService settings)
        {
            _settings = settings;
        }


        public IObservable<String> UploadMultiMediaObjectRawData(Client.MultimediaObject mmo)
        {
            var res = Observable.FromEvent<EventHandler<SubmitCompletedEventArgs>, SubmitCompletedEventArgs>((a) => (s, args) => a(args), d => _msc.SubmitCompleted += d, d => _msc.SubmitCompleted -= d)
             .Select(args => args.Result)
             .Take(1);

            byte[] data;
            // Read the entire image in one go into a byte array
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {

                using (IsolatedStorageFileStream isfs = isf.OpenFile(mmo.Uri, FileMode.Open, FileAccess.Read))
                {
                    // Allocate an array large enough for the entire file
                    data = new byte[isfs.Length];
                    // Read the entire file and then close it
                    isfs.Read(data, 0, data.Length);                    
                }

            }
            UserCredentials cred = _settings.getSettings().ToCreds();
            _msc.SubmitAsync(mmo.Uri, mmo.Uri, mmo.MediaType.ToString(), 0, 0, 0, cred.LoginName, DateTime.Now.ToShortDateString(),cred.ProjectID, data);
            return res;
        }
    }
}
