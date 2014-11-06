using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reactive.Linq;
using System.ServiceModel;

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

            return string.Format("DM-{0}-{1}-{2}-{3}.{4}",
                ownerCode,
                CollectionOwnerID,
                mmo.TimeCreated.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                mmo.TimeCreated.ToString("HHmmss", CultureInfo.InvariantCulture),
                extension
                );
        }

        public IObservable<Uri> UploadMultimedia(MultimediaObject mmo, Stream data)
        {
            var maxPackageSize = 4193000; // < 4 MB -> Submit, else chunked upload
            var uri = string.Empty;
            var packageBuffer = new byte[maxPackageSize];
            var rolledBack = false;

            Action readPackage = () =>
            {
                var packageSize = data.Read(packageBuffer, 0, packageBuffer.Length);
                if (packageSize < packageBuffer.Length)
                {
                    Array.Resize(ref packageBuffer, packageSize);
                }
            };

            Action disposeStream = () =>
            {
                if (data != null)
                {
                    data.Dispose();
                }
            };
            var res =
                Observable.Merge(
                    UploadMultimediaCompleted.FilterByUserState(mmo).Select(x => x as AsyncCompletedEventArgs),
                    BeginTransactionCompleted.FilterByUserState(mmo).Select(x => x as AsyncCompletedEventArgs),
                    EncodeFileCompleted.FilterByUserState(mmo).Select(x => x as AsyncCompletedEventArgs),
                    CommitCompleted.FilterByUserState(mmo).Select(x => x as AsyncCompletedEventArgs),
                    RollbackCompleted.FilterByUserState(mmo)
                )
                .Select(p =>
                    {
                        if (p.Error == null)
                        {
                            if (p is MultimediaService.BeginTransactionCompletedEventArgs)
                            {
                                // Result is a fake URI -> ignore and start first package upload
                                // Package Data is still in the buffer
                                _multimedia.EncodeFileAsync(packageBuffer, mmo);
                                return null;
                            }
                            else if (p is MultimediaService.CommitCompletedEventArgs)
                            {
                                uri = (p as MultimediaService.CommitCompletedEventArgs).Result;
                            }
                            else if (p is MultimediaService.SubmitCompletedEventArgs)
                            {
                                uri = (p as MultimediaService.SubmitCompletedEventArgs).Result;
                            }
                            else if (p is MultimediaService.EncodeFileCompletedEventArgs)
                            {
                                readPackage();
                                if (packageBuffer.Length > 0)
                                {
                                    // Send next package
                                    _multimedia.EncodeFileAsync(packageBuffer, mmo);
                                }
                                else
                                {
                                    // Commit transaction
                                    _multimedia.CommitAsync(mmo);
                                }
                                return null;
                            }
                            else
                            {
                                // Rollback Completed successfully -> Alert the user.
                                throw new ServiceOperationException("Upload failed, operation rolled back, please retry.");
                            }

                            try
                            {
                                return new Uri(uri, UriKind.Absolute);
                            }
                            catch (Exception ex)
                            {
                                int errorCode;

                                if (int.TryParse(uri, out errorCode))
                                {
                                    throw new ServiceOperationException(string.Format("Multimedia Upload failed with error code: {0}", errorCode));
                                }

                                throw new ServiceOperationException("Service returned invalid Image URI", ex);
                            }
                        }
                        else
                        {
                            if (p.Error is FaultException && !rolledBack)
                            {
                                rolledBack = true;
                                _multimedia.RollbackAsync(mmo);
                                return null;
                            }
                            throw new ServiceOperationException(p.Error.Message, p.Error);
                        }
                    })
                    .Where(x => x != null)
                    .ConvertToServiceErrors()
                    .Finally(disposeStream)
                    .Take(1)
                    .Replay(1);
            var collectionOwnerID = Mapping.EnsureKey(mmo.OwnerType, mmo.RelatedId);
            var filename = ServiceFileName(mmo, collectionOwnerID);

            res.Connect();

            readPackage();

            WithCredentials(login =>
            {
                if (packageBuffer.Length < maxPackageSize)
                {
                    // Stream only contained enough for one package -> Submit with one service call
                    _multimedia.SubmitAsync(Guid.NewGuid().ToString(), filename, mmo.MediaType.ToString(), 0, 0, 0, login.LoginName, mmo.TimeCreated.ToString(CultureInfo.InvariantCulture), login.ProjectID, packageBuffer, mmo);
                }
                else
                {
                    // Start Chunked upload
                    _multimedia.BeginTransactionAsync(Guid.NewGuid().ToString(), filename, mmo.MediaType.ToString(), 0, 0, 0, login.LoginName, mmo.TimeCreated.ToString(CultureInfo.InvariantCulture), login.ProjectID, mmo);
                }
            });

            return res;
        }
    }
}