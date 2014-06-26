using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Globalization;
using System.Reactive.Linq;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient
    {
        private string ServiceFileName(MultimediaObject mmo, int CollectionOwnerID)
        {
            string extension;
            switch (mmo.MediaType)
            {
                case MediaType.Image:
                    extension = "jpg";
                    break;
                case MediaType.Audio:
                    extension = "wav";
                    break;
                case MediaType.Video:
                    extension = "mp4";
                    break;
                default:
                    throw new ArgumentException("Unknown Media Type");
            }

            string ownerCode;
            switch (mmo.OwnerType)
            {               
                case DBObjectType.Event:
                    ownerCode = "EV";
                    break;
                case DBObjectType.Specimen:
                    ownerCode = "SP";
                    break;
                case DBObjectType.IdentificationUnit:
                    ownerCode = "IU";
                    break;
                default:
                    throw new ArgumentException("Unsupported Media Owner");
            }

            return string.Format("DM-{0}-{1}-{2}-{3}-{4}.{5}",
                ownerCode,
                CollectionOwnerID,
                mmo.TimeCreated.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                mmo.TimeCreated.ToString("HHmmss", CultureInfo.InvariantCulture),
                DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                extension
                );
        }

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
            var collectionOwnerID = Mapping.EnsureKey(mmo.OwnerType, mmo.RelatedId);
            var filename = ServiceFileName(mmo, collectionOwnerID);
            _multimedia.SubmitAsync(Guid.NewGuid().ToString(), filename, mmo.MediaType.ToString(), 0, 0, 0, login.LoginName,mmo.TimeCreated.ToString(CultureInfo.InvariantCulture), login.ProjectID, data, mmo);
            return res;
        }
    }
}
