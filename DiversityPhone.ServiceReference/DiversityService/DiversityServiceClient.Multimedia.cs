using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient
    {
        public IObservable<Uri> UploadMultimedia(MultimediaObject mmo, byte[] data)
        {
            var login = this.GetCreds();
            var res = UploadMultimediaCompleted.MakeObservableServiceResultSingle(mmo)                
                .Select(p =>
                    {
                        var uriString = p.Result;
                        if (!string.IsNullOrWhiteSpace(uriString) && uriString.ToLowerInvariant().StartsWith("http://"))
                            return new Uri(p.Result, UriKind.Absolute);
                        else
                            throw new ServiceOperationException(p.Result);
                    });
            var filename = mmo.Uri.Split(new char[] { '/', '\\' }).Last();
            _multimedia.SubmitAsync(filename, filename, mmo.MediaType.ToString(), 0, 0, 0, login.LoginName, DateTime.Now.ToShortDateString(), login.ProjectID, data, mmo);
            return res;
        }
    }
}
