using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.Media;
using Microsoft.Phone.Tasks;
using Microsoft.Live;
using System.IO;
using DiversityPhone.Services;
using Ninject;
using ReactiveUI;
using DiversityPhone.Model;

namespace DiversityPhone.View
{
    public partial class Admin : PhoneApplicationPage
    {
        Microsoft.Live.LiveConnectSession _Session;

        public Admin()
        {
            InitializeComponent();
        }

        private void ExportDatabase(object sender, RoutedEventArgs e)
        {
            if (_Session != null)
            {
                var client = new LiveConnectClient(_Session);


                var store = IsolatedStorageFile.GetUserStoreForApplication();                
                var dbFiles = new Stack<string>();
                dbFiles.Push(DiversityDataContext.DB_FILENAME);
                foreach (var f in FieldDataService.BackupDBFiles().TakeWhile(store.FileExists))
                {
                    dbFiles.Push(f);
                }
                                
                var file = dbFiles.Pop();
                var fileStream = store.OpenFile(file, FileMode.Open, FileAccess.Read, FileShare.Read);

                client.UploadProgressChanged += (s, args) => SystemTray.ProgressIndicator.Value = args.ProgressPercentage;
                client.UploadCompleted += (s, args) => 
                {
                    fileStream.Dispose();


                    if (dbFiles.Count > 0)
                    {
                        store.DeleteFile(file);
                        file = dbFiles.Pop();
                        fileStream = store.OpenFile(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                        client.UploadAsync("me/skydrive/my_documents", file, fileStream, OverwriteOption.Rename, file);
                    }
                    else
                    {
                        store.Dispose();
                        SystemTray.ProgressIndicator.IsVisible = false;
                    }
                };
                SystemTray.ProgressIndicator.IsVisible = true;
                client.UploadAsync("me/skydrive/my_documents", file, fileStream, OverwriteOption.Rename, file);

            }
        }

        private void ImportDatabase(object sender, RoutedEventArgs e)
        {
            if (_Session != null)
            {
                
                var client = new LiveConnectClient(_Session);
                client.GetCompleted += (s, args) => 
                    {
                        if(args.Error == null && !args.Cancelled)
                        {
                            var myDocs = args.Result["data"] as IList<object>;

                            var db = (from i in myDocs
                                     let item = i as IDictionary<string, object>
                                     where item["type"].ToString() == "file"
                                        && item["name"].ToString() == DiversityDataContext.DB_FILENAME
                                      select item["id"].ToString()).FirstOrDefault();
                            if (db != null)
                            {
                                client.DownloadAsync(db + "/content" );
                                SystemTray.ProgressIndicator.IsVisible = true;
                            }
                        }
                    };

                client.DownloadProgressChanged += (s, args) => SystemTray.ProgressIndicator.Value = args.ProgressPercentage;
                client.DownloadCompleted += async (s, args) =>
                {
                    if (args.Error == null && !args.Cancelled)
                    {
                        using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            if (store.FileExists(DiversityDataContext.DB_FILENAME))
                                store.DeleteFile(DiversityDataContext.DB_FILENAME);
                            using (var fileStream = store.OpenFile(DiversityDataContext.DB_FILENAME, FileMode.CreateNew, FileAccess.Write))
                            {
                                await args.Result.CopyToAsync(fileStream);
                            }
                        }
                    }
                    SystemTray.ProgressIndicator.IsVisible = false;

                    App.Kernel.Get<FieldDataService>().CheckAndRepairDatabase();

                };


                client.GetAsync("me/skydrive/my_documents/files");

            }
        }

        private void SignInButton_SessionChanged(object sender, Microsoft.Live.Controls.LiveConnectSessionChangedEventArgs e)
        {
            if (e.Status == Microsoft.Live.LiveConnectSessionStatus.Connected)
                _Session = e.Session;
            else
                _Session = null;
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RxApp.MessageBus.SendMessage(EventMessage.Default, MessageContracts.INIT);
        }
    }
}