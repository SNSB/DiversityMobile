using DiversityPhone.Model;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using DiversityPhone.Interface;

namespace DiversityPhone
{


    public interface IStoreImages : IStoreMultimedia
    {
        string StoreImage(string fileNameHint, PhotoResult image);
        Stream GetImageThumbnail(string fileName);
    }

    public static class MultimediaFileNameMixin
    {
        /// <summary>
        /// Generates a unique Name for a Multimedia File based on the type and owner of the MMO
        /// as well as a TimeStamp
        /// </summary>
        public static string NewFileName(this MultimediaObject This)
        {
            string extension = "jpg";

            switch (This.MediaType)
            {

                case MediaType.Audio:
                    extension = "wav";
                    break;
                case MediaType.Video:
                    extension = "mp4";
                    break;
                case MediaType.Image:
                    break;
                default:
                    break;
            }

            return string.Format("{0}_{1}_{2}.{3}",
                This.OwnerType,
                This.RelatedId,
                DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture),
                extension);
        }
    }

    public static class CameraRollMixin
    {
        public static PictureAlbum CameraRoll(this MediaLibrary library)
        {
            return library.RootPictureAlbum.Albums.Where(a => a.Name == "Camera Roll" || a.Name == "Kamerarolle").FirstOrDefault();
        }
    }

    public class MultimediaStorageService : IStoreImages
    {
        enum StorageType
        {
            IsolatedStorage,
            CameraRoll,
            Unknown
        }

        struct StorageDescriptor
        {
            public StorageType Type { get; set; }
            public string FileName { get; set; }
        }

        private readonly Regex ISOSTORE_URI_REGEX = new Regex("^isostore:(?<name>.+)");
        private const string ISOSTORE_URI_FORMAT = "isostore:{0}";
        private const string ISOSTORE_MEDIA_FOLDER_FORMAT = "/multimedia/{0}";

        private readonly Regex CAMERAROLL_URI_REGEX = new Regex("^cameraroll:/(?<name>.+)");
        private const string CAMERAROLL_URI_FORMAT = "cameraroll:/{0}";

        private StorageDescriptor ParseUri(string uri)
        {
            var matches = ISOSTORE_URI_REGEX.Match(uri);
            if (matches.Success)
            {
                return new StorageDescriptor() { Type = StorageType.IsolatedStorage, FileName = matches.Groups["name"].Value };
            }

            matches = CAMERAROLL_URI_REGEX.Match(uri);
            if (matches.Success)
            {
                return new StorageDescriptor() { Type = StorageType.CameraRoll, FileName = matches.Groups["name"].Value };
            }

            return new StorageDescriptor() { Type = StorageType.Unknown, FileName = string.Empty };
        }

        private static Version ApolloVersion = new Version(8, 0);

        private static bool IsWP8 { get { return Environment.OSVersion.Version >= ApolloVersion; } }

        private MediaLibrary Library
        {
            get
            {
                return new MediaLibrary();
            }
        }



        public string StoreImage(string fileNameHint, PhotoResult image)
        {
            string filename = string.Empty;
            //On WP8 the image is probably already saved, check for that...
            if (IsWP8)
            {
                var lib = Library;
                var crImage = lib.CameraRoll().Pictures.OrderByDescending(p => p.Date).First();
                filename = crImage.Name;
            }
            else
            {
                var pic = Library.SavePictureToCameraRoll(fileNameHint, image.ChosenPhoto);
                filename = pic.Name;
            }
            return string.Format(CAMERAROLL_URI_FORMAT, filename);
        }

        public Stream GetImageThumbnail(string fileUri)
        {
            var storageDescriptor = ParseUri(fileUri);

            if (storageDescriptor.Type == StorageType.CameraRoll)
            {
                var fileName = storageDescriptor.FileName;
                var picture = Library.CameraRoll().Pictures.Where(p => p.Name == fileName).FirstOrDefault();
                if (picture != null)
                {
                    return picture.GetThumbnail();
                }
            }
            return Stream.Null;
        }

        private Stream GetMultimediaFromCameraRoll(string fileName)
        {
            var picture = Library.CameraRoll().Pictures.Where(p => p.Name == fileName).FirstOrDefault();
            if (picture != null)
            {
                return picture.GetImage();
            }
            return Stream.Null;
        }

        private Stream GetMultimediaFromIsolatedStorage(string fileName)
        {
            var result = Stream.Null;
            UsingIsolatedStorage(store =>
            {
                if (store.FileExists(fileName))
                {
                    result = store.OpenFile(fileName, FileMode.Open, FileAccess.Read);
                }
            });
            return result;
        }


        public string StoreMultimedia(string fileName, Stream data)
        {
            var filePath = string.Format(ISOSTORE_MEDIA_FOLDER_FORMAT, fileName);
            UsingIsolatedStorage(store =>
                {
                    if (store.FileExists(filePath))
                    {
                        throw new InvalidOperationException("Couldn't save Media. File already Exists!");
                    }
                    else
                    {
                        using (var file = store.CreateFile(filePath))
                        {
                            data.Seek(0, SeekOrigin.Begin);
                            data.CopyTo(file);
                            file.Flush();
                        }
                    }
                });
            return string.Format(ISOSTORE_URI_FORMAT, filePath);
        }

        public Stream GetMultimedia(string filePath)
        {
            var storageDescriptor = ParseUri(filePath);

            switch (storageDescriptor.Type)
            {
                case StorageType.IsolatedStorage:
                    return GetMultimediaFromIsolatedStorage(storageDescriptor.FileName);
                case StorageType.CameraRoll:
                    return GetMultimediaFromCameraRoll(storageDescriptor.FileName);
                case StorageType.Unknown:
                    break;
                default:
                    break;
            }
            return Stream.Null;
        }

        public void ClearMultimedia()
        {
            UsingIsolatedStorage(store =>
                {
                    var mediaFolder = string.Format(ISOSTORE_MEDIA_FOLDER_FORMAT, string.Empty);
                    if (store.DirectoryExists(mediaFolder))
                    {
                        foreach (var file in store.GetFileNames(mediaFolder))
                        {
                            store.DeleteFile(string.Format(ISOSTORE_MEDIA_FOLDER_FORMAT, file));
                        }
                    }
                });
        }


        public void DeleteMultimedia(string uri)
        {
            var storageDescriptor = ParseUri(uri);

            if (storageDescriptor.Type == StorageType.IsolatedStorage)
            {
                UsingIsolatedStorage(store =>
                    {
                        if (store.FileExists(storageDescriptor.FileName))
                            store.DeleteFile(storageDescriptor.FileName);
                    });

            }
        }

        private static void UsingIsolatedStorage(Action<IsolatedStorageFile> action)
        {
            using (var isostore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                action(isostore);
            }
        }
    }
}
