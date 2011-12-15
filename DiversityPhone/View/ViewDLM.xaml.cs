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
using Microsoft.Phone.BackgroundTransfer;
using System.IO.IsolatedStorage;
using DiversityPhone.MapServiceReference;

namespace DiversityPhone.View
{
    public partial class ViewDLM : PhoneApplicationPage
    {

        private MapServiceClient mapinfo;
        private IList<String> maps;
        private IList<String> filteredMaps;

        public ViewDLM()
        {
            InitializeComponent();
            mapinfo = new MapServiceClient();
            mapinfo.GetMapListAsync();
            mapinfo.GetMapListCompleted += new EventHandler<MapServiceReference.GetMapListCompletedEventArgs>(mapinfo_GetMapListCompleted);
            mapinfo.GetMapUrlCompleted+=new EventHandler<GetMapUrlCompletedEventArgs>(mapinfo_GetMapUrlCompleted);
            mapinfo.GetXmlUrlCompleted+=new EventHandler<GetXmlUrlCompletedEventArgs>(mapinfo_GetXmlUrlCompleted);
            // Bind the list of urls to the ListBox
            

            // Make sure that the required "transfers" directory exists
            // in isolated storage.
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("Shared/Transfers/Maps/MapImages"))
                {
                    isoStore.CreateDirectory("Shared/Transfers/Maps/MapImages");
                }
                if (!isoStore.DirectoryExists("Shared/Transfers/Maps/XML"))
                {
                    isoStore.CreateDirectory("Shared/Transfers/Maps/XML");
                }
            }
        }

        #region asynchronous Events

        public void mapinfo_GetMapListCompleted(object sender, MapServiceReference.GetMapListCompletedEventArgs e)
        {
            maps = e.Result;
            filterMaps();
        }

        public void mapinfo_GetMapUrlCompleted(object sender, MapServiceReference.GetMapUrlCompletedEventArgs e)
        {
            // Check to see if the maximum number of requests per app has been exceeded.
            if (BackgroundTransferService.Requests.Count() >= 5)
            {
                // Note: Instead of showing a message to the user, you could store the
                // requested file URI in isolated storage and add it to the queue later.
                MessageBox.Show("The maximum number of background file transfer requests for this application has been exceeded. ");
                return;
            }

            string transferFileName = e.Result;
            Uri transferUri = new Uri(Uri.EscapeUriString(transferFileName), UriKind.RelativeOrAbsolute);

            // Create the new transfer request, passing in the URI of the file to 
            // be transferred.
            BackgroundTransferRequest transferRequest = new BackgroundTransferRequest(transferUri);

            // Set the transfer method. GET and POST are supported.
            transferRequest.Method = "GET";

            // Get the file name from the end of the transfer Uri and create a local Uri 
            // in the "transfers" directory in isolated storage.
            string downloadFile = transferFileName.Substring(transferFileName.LastIndexOf("/") + 1);
            Uri downloadUri = new Uri("Shared/Transfers/Maps/MapImages/" + downloadFile, UriKind.RelativeOrAbsolute);
            transferRequest.DownloadLocation = downloadUri;

            // Pass custom data with the Tag property. This value cannot be more than 4000 characters.
            // In this example, the friendly name for the file is passed. 
            transferRequest.Tag = downloadFile;

            // If the WiFi-only checkbox is not checked, then set the TransferPreferences
            // to allow transfers over a cellular connection.
            if (wifiOnlyCheckbox.IsChecked == false)
            {
                transferRequest.TransferPreferences = TransferPreferences.AllowCellular;
            }
            if (externalPowerOnlyCheckbox.IsChecked == false)
            {
                transferRequest.TransferPreferences = TransferPreferences.AllowBattery;
            }
            if (wifiOnlyCheckbox.IsChecked == false && externalPowerOnlyCheckbox.IsChecked == false)
            {
                transferRequest.TransferPreferences = TransferPreferences.AllowCellularAndBattery;
            }

            // Add the transfer request using the BackgroundTransferService. Do this in 
            // a try block in case an exception is thrown.
            try
            {
                BackgroundTransferService.Add(transferRequest);
            }
            catch (InvalidOperationException ex)
            {
                // TBD - update when exceptions are finalized
                MessageBox.Show("Unable to add background transfer request. " + ex.Message);
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to add background transfer request.");
            }

            //Todo delete corresponding request/file by failure
        }

        public void mapinfo_GetXmlUrlCompleted(object sender, MapServiceReference.GetXmlUrlCompletedEventArgs e)
        {
            // Check to see if the maximum number of requests per app has been exceeded.
            if (BackgroundTransferService.Requests.Count() >= 5)
            {
                // Note: Instead of showing a message to the user, you could store the
                // requested file URI in isolated storage and add it to the queue later.
                MessageBox.Show("The maximum number of background file transfer requests for this application has been exceeded. ");
                return;
            }

            string transferFileName = e.Result;
            Uri transferUri = new Uri(Uri.EscapeUriString(transferFileName), UriKind.RelativeOrAbsolute);

            // Create the new transfer request, passing in the URI of the file to 
            // be transferred.
            BackgroundTransferRequest transferRequest = new BackgroundTransferRequest(transferUri);

            // Set the transfer method. GET and POST are supported.
            transferRequest.Method = "GET";

            // Get the file name from the end of the transfer Uri and create a local Uri 
            // in the "transfers" directory in isolated storage.
            string downloadFile = transferFileName.Substring(transferFileName.LastIndexOf("/") + 1);
            Uri downloadUri = new Uri("Shared/Transfers/Maps/XML/" + downloadFile, UriKind.RelativeOrAbsolute);
            transferRequest.DownloadLocation = downloadUri;

            // Pass custom data with the Tag property. This value cannot be more than 4000 characters.
            // In this example, the friendly name for the file is passed. 
            transferRequest.Tag = downloadFile;

            // If the WiFi-only checkbox is not checked, then set the TransferPreferences
            // to allow transfers over a cellular connection.
            if (wifiOnlyCheckbox.IsChecked == false)
            {
                transferRequest.TransferPreferences = TransferPreferences.AllowCellular;
            }
            if (externalPowerOnlyCheckbox.IsChecked == false)
            {
                transferRequest.TransferPreferences = TransferPreferences.AllowBattery;
            }
            if (wifiOnlyCheckbox.IsChecked == false && externalPowerOnlyCheckbox.IsChecked == false)
            {
                transferRequest.TransferPreferences = TransferPreferences.AllowCellularAndBattery;
            }

            // Add the transfer request using the BackgroundTransferService. Do this in 
            // a try block in case an exception is thrown.
            try
            {
                BackgroundTransferService.Add(transferRequest);
            }
            catch (InvalidOperationException ex)
            {
                // TBD - update when exceptions are finalized
                MessageBox.Show("Unable to add background transfer request. " + ex.Message);
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to add background transfer request.");
            }
            //Todo delete corresponding request/file by failure
        }
        #endregion

        #region UI-Events
        private void mapText_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //TODO
            //Show Details of the Map
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {

            //TODO
            //Check if file is already present
            //Check if file is already in the dowload process

            // Check to see if the maximum number of requests per app has been exceeded.
            if (BackgroundTransferService.Requests.Count() >= 4)
            {
                // Note: Instead of showing a message to the user, you could store the
                // requested file URI in isolated storage and add it to the queue later.
                MessageBox.Show("The maximum number of background file transfer requests for this application has been exceeded. ");
                return;
            }

            //Get correponding URL for the map
            mapinfo.GetMapUrlAsync(((Button)sender).Tag as string);
            mapinfo.GetXmlUrlAsync(((Button)sender).Tag as string);

        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            filterMaps();
        }

        private void filterMaps()
        {
            if (maps != null)
            {
                if (textBox1.Text==null|| textBox1.Text.Equals(String.Empty))
                    filteredMaps = maps;
                else
                {
                    filteredMaps = new List<String>();
                    foreach (String mapname in maps)
                    {
                        if (mapname.ToLower().Contains(textBox1.Text.ToLower()))
                            filteredMaps.Add(mapname);
                    }
                }
            }
            URLListBox.ItemsSource = filteredMaps;
        }
        #endregion
    }
}