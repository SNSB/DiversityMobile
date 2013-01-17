using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Svc = DiversityPhone.DiversityService;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient
    {
        public IObservable<String> UploadMultimedia(MultimediaObject mmo, byte[] data)
        {
            var login = this.GetCreds();
            var res = FilterByUserStatePipeErrorsAndReplay(UploadMultimediaCompleted, mmo)
                .Select(p => p.Result);
            _multimedia.SubmitAsync(mmo.Uri, mmo.Uri, mmo.MediaType.ToString(), 0, 0, 0, login.LoginName, DateTime.Now.ToShortDateString(), login.ProjectID, data, mmo);
            return res;
        }
    }
}
