using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using Microsoft.Phone.BackgroundTransfer;
using System.Xml.Linq;
using DiversityPhone.ViewModels;
using DiversityPhone.Model;


namespace DiversityPhone.View
{
    public partial class ViewLM : PhoneApplicationPage
    {
        private ViewLMVM VM { get { return DataContext as ViewLMVM; } }

        // List of BackgroundTransferRequest objects that will be bound
        // to the ListBox in MainPage.xaml
        private IEnumerable<BackgroundTransferRequest> transferRequests;

        // Booleans for tracking if any transfers are waiting for user
        // action. 
        private bool WaitingForExternalPower;
        private bool WaitingForExternalPowerDueToBatterySaverMode;
        private bool WaitingForNonVoiceBlockingNetwork;
        private bool WaitingForWiFi;
        private Dictionary<String,String> savedMaps;
        public ViewLM()
        {
            InitializeComponent();
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("Maps/MapImages"))
                {
                    isoStore.CreateDirectory("Maps/MapImages");
                }
                if (!isoStore.DirectoryExists("Maps/XML"))
                {
                    isoStore.CreateDirectory("Maps/XML");
                }
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Reset all of the user action booleans on page load.
            WaitingForExternalPower = false;
            WaitingForExternalPowerDueToBatterySaverMode = false;
            WaitingForNonVoiceBlockingNetwork = false;
            WaitingForWiFi = false;
            savedMaps = new Dictionary<String, String>();
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IList<String> mapimages = isoStore.GetFileNames("Maps\\MapImages\\*");
                foreach (String mapimage in mapimages)
                {
                    String name=mapimage.Substring(0,mapimage.LastIndexOf("."));
                    String xmlname = name + ".xml";
                    if (isoStore.FileExists("Maps\\XML\\" + xmlname))
                    {
                        ImageOptions io=ImageOptions.loadImagesOptionsFromFile("Maps\\XML\\" + xmlname);
                        savedMaps.Add(name,name+" - "+io.Description);
                    }
                }
            }
            SavedMapsListBox.ItemsSource = savedMaps;
            // When the page loads, refresh the list of file transfers.
            InitialTansferStatusCheck();
            UpdateUI();
        }

        private void InitialTansferStatusCheck()
        {
            UpdateRequestsList();
            foreach (var transfer in transferRequests)
            {
                transfer.TransferStatusChanged += new EventHandler<BackgroundTransferEventArgs>(transfer_TransferStatusChanged);
                transfer.TransferProgressChanged += new EventHandler<BackgroundTransferEventArgs>(transfer_TransferProgressChanged);
                ProcessTransfer(transfer);
            }

            if (WaitingForExternalPower)
            {
                MessageBox.Show("You have one or more file transfers waiting for external power. Connect your device to external power to continue transferring.");
            }
            if (WaitingForExternalPowerDueToBatterySaverMode)
            {
                MessageBox.Show("You have one or more file transfers waiting for external power. Connect your device to external power or disable Battery Saver Mode to continue transferring.");
            }
            if (WaitingForNonVoiceBlockingNetwork)
            {
                MessageBox.Show("You have one or more file transfers waiting for a network that supports simultaneous voice and data.");
            }
            if (WaitingForWiFi)
            {
                MessageBox.Show("You have one or more file transfers waiting for a WiFi connection. Connect your device to a WiFi network to continue transferring.");
            }

        }

        void transfer_TransferStatusChanged(object sender, BackgroundTransferEventArgs e)
        {
            ProcessTransfer(e.Request);
            UpdateUI();
        }

        private void ProcessTransfer(BackgroundTransferRequest transfer)
        {
            switch (transfer.TransferStatus)
            {
                case TransferStatus.Completed:
                    // If the status code of a completed transfer is 200 or 206, the
                    // transfer was successful
                    if (transfer.StatusCode == 200 || transfer.StatusCode == 206)
                    {
                        // Remove the transfer request in order to make room in the 
                        // queue for more transfers. Transfers are not automatically
                        // removed by the system.
                        RemoveTransferRequest(transfer.RequestId);

                        // In this example, the downloaded file is moved into the root
                        // Isolated Storage directory
                        using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            string filename = transfer.Tag;
                            string path = transfer.DownloadLocation.OriginalString.Substring(0, transfer.DownloadLocation.OriginalString.LastIndexOf("\\"));

                            if (path.Contains("MapImages"))
                            {
                                string fullFilename = "Maps\\MapImages\\" + filename;
                                if (isoStore.FileExists(fullFilename))
                                {
                                    isoStore.DeleteFile(fullFilename);
                                }
                                isoStore.MoveFile(transfer.DownloadLocation.OriginalString, fullFilename);
                                //Add map to the savedMapsList
                                String name = filename.Substring(0, filename.LastIndexOf("."));
                                String xmlname = name + ".xml";
                                if (isoStore.FileExists("Maps\\XML\\" + xmlname))
                                    if (!savedMaps.ContainsKey(name))
                                    {
                                        ImageOptions io=ImageOptions.loadImagesOptionsFromFile("Maps\\XML\\" + xmlname);
                                        string description = io.Description;
                                        savedMaps.Add(name, name + " - " + description);
                                    }
                            }
                            else if (path.Contains("XML"))
                            {   
                                string fullFilename = "Maps\\XML\\" + filename;
                                if (isoStore.FileExists(fullFilename))
                                {
                                    isoStore.DeleteFile(fullFilename);
                                }
                                isoStore.MoveFile(transfer.DownloadLocation.OriginalString, fullFilename);
                                //Add map to the savedMapsList
                                String name = filename.Substring(0, filename.LastIndexOf("."));
                                string[] mapimages=isoStore.GetFileNames("Maps\\MapImages\\*");
                                foreach (String mapimage in mapimages)
                                {
                                    if (mapimage.Contains(name))
                                    {
                                        if (!savedMaps.ContainsKey(name))
                                        {
                                            ImageOptions io = ImageOptions.loadImagesOptionsFromFile(fullFilename);
                                            string description = io.Description;
                                            savedMaps.Add(name, name + " - " + description);
                                        }
                                        break;
                                    }
                                }   
                            }
                        }
                    }
                    else
                    {
                        // This is where you can handle whatever error is indicated by the
                        // StatusCode and then remove the transfer from the queue. 
                        RemoveTransferRequest(transfer.RequestId);
                        if (transfer.TransferError != null)
                        {
                            // Handle TransferError, if there is one.
                        }
                    }
                    break;

                case TransferStatus.WaitingForExternalPower:
                    WaitingForExternalPower = true;
                    break;

                case TransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                    WaitingForExternalPowerDueToBatterySaverMode = true;
                    break;

                case TransferStatus.WaitingForNonVoiceBlockingNetwork:
                    WaitingForNonVoiceBlockingNetwork = true;
                    break;

                case TransferStatus.WaitingForWiFi:
                    WaitingForWiFi = true;
                    break;
            }
        }

        void transfer_TransferProgressChanged(object sender, BackgroundTransferEventArgs e)
        {
            UpdateUI();
        }


        

        private void UpdateRequestsList()
        {
            // The Requests property returns new references, so make sure that
            // you dispose of the old references to avoid memory leaks.
            if (transferRequests != null)
            {
                foreach (var request in transferRequests)
                {
                    request.Dispose();
                }
            }
            transferRequests = BackgroundTransferService.Requests;
        }

        private void UpdateUI()
        {
            // Update the list of transfer requests
            UpdateRequestsList();
            // If there are 1 or more transfers, hide the "no transfers"
            // TextBlock. IF there are zero transfers, show the TextBlock.
            if (transferRequests.Count<BackgroundTransferRequest>() > 0)
            {
                EmptyTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyTextBlock.Visibility = Visibility.Visible;
            }
            // Update the TransferListBox with the list of transfer requests.
            TransferListBox.ItemsSource = transferRequests;
        }

        private void RemoveTransferRequest(string transferID)
        {
            // Use Find to retrieve the transfer request with the specified ID.
            BackgroundTransferRequest transferToRemove = BackgroundTransferService.Find(transferID);

            // try to remove the transfer from the background transfer service.
            try
            {
                BackgroundTransferService.Remove(transferToRemove);
            }
            catch (Exception)
            {

            }
        }

        private void LoadMaps_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.AddMaps.Execute(null);
        }


        private void CancelAllButton_Click(object sender, EventArgs e)
        {
            // Loop through the list of transfer requests
            foreach (var transfer in BackgroundTransferService.Requests)
            {
                RemoveTransferRequest(transfer.RequestId);
            }

            // Refresh the list of file transfer requests.
            UpdateUI();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // The ID for each transfer request is bound to the
            // Tag property of each Remove button.
            string transferID = ((Button)sender).Tag as string;

            // Delete the transfer request
            RemoveTransferRequest(transferID);

            // Refresh the list of file transfers
            UpdateUI();
        }
    }
}