using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.BackgroundTransfer;
using System.IO.IsolatedStorage;
using DiversityPhone.PhoneMediaService;

namespace DiversityPhone.View
{
    public partial class ViewDLM : PhoneApplicationPage
    {

        private PhoneMediaServiceClient mapinfo;
        private IList<String> maps;
        private IList<String> filteredMaps;

        public ViewDLM()
        {
            InitializeComponent();
            mapinfo = new PhoneMediaServiceClient();
            mapinfo.GetMapListAsync();
            mapinfo.GetMapListCompleted += new EventHandler<GetMapListCompletedEventArgs>(mapinfo_GetMapListCompleted);
            mapinfo.GetMapUrlCompleted+=new EventHandler<GetMapUrlCompletedEventArgs>(mapinfo_GetMapUrlCompleted);
            mapinfo.GetXmlUrlCompleted+=new EventHandler<GetXmlUrlCompletedEventArgs>(mapinfo_GetXmlUrlCompleted);
          

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

        public void mapinfo_GetMapListCompleted(object sender, GetMapListCompletedEventArgs e)
        {
            maps = e.Result;
            filterMaps();
        }

        public void mapinfo_GetMapUrlCompleted(object sender, GetMapUrlCompletedEventArgs e)
        {
            //Thus method is called after the Add-Button_Click Event which selects a map.

            // Check to see if the maximum number of requests per app has been exceeded.
            if (BackgroundTransferService.Requests.Count() >= 5)
            {
                // Note: Instead of showing a message to the user, you could store the
                // requested file URI in isolated storage and add it to the queue later.
                MessageBox.Show("The maximum number of background file transfer requests for this application has been exceeded. ");

                //Todo: Wait for xml Transfer. Use a RequestList
                return;
            }
            
            //The Result of the selection is passed in the Arguments of the event
            string transferFileName = e.Result;
            Uri transferUri = new Uri(Uri.EscapeUriString(transferFileName), UriKind.RelativeOrAbsolute);

            // Create the new transfer request, passing in the URI of the file to 
            // be transferred.
            BackgroundTransferRequest transferRequest = new BackgroundTransferRequest(transferUri);

            // Set the transfer method.
            transferRequest.Method = "GET";

            // Get the file name from the end of the transfer Uri and create a local Uri 
            // in isolated storage.
            string downloadFile = transferFileName.Substring(transferFileName.LastIndexOf("/") + 1);

            //By specification operations with the Backgroundtransferservice need to be stored in Shared/Tranfers
            Uri downloadUri = new Uri("Shared/Transfers/Maps/MapImages/" + downloadFile, UriKind.RelativeOrAbsolute);
            transferRequest.DownloadLocation = downloadUri;

            // Pass custom data with the Tag property. This value cannot be more than 4000 characters.
            // In this example, the friendly name for the file is passed. 
            transferRequest.Tag = downloadFile;

            //Set the authorization in  the header of the request
            string credentials = Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes("snsb" + ":" + "maps"));
            transferRequest.Headers.Add("Authorization", "Basic " + credentials);
            

            //Check transfer preferences
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
                MessageBox.Show("Unable to add background transfer request. " + ex.Message);
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to add background transfer request.");
            }
        }

        public void mapinfo_GetXmlUrlCompleted(object sender, GetXmlUrlCompletedEventArgs e)
        {
            if (BackgroundTransferService.Requests.Count() >= 5)
            {
                MessageBox.Show("The maximum number of background file transfer requests for this application has been exceeded. ");
                return;
            }

            string transferFileName = e.Result;
            Uri transferUri = new Uri(Uri.EscapeUriString(transferFileName), UriKind.RelativeOrAbsolute);
            BackgroundTransferRequest transferRequest = new BackgroundTransferRequest(transferUri);
            transferRequest.Method = "GET";
            string downloadFile = transferFileName.Substring(transferFileName.LastIndexOf("/") + 1);
            Uri downloadUri = new Uri("Shared/Transfers/Maps/XML/" + downloadFile, UriKind.RelativeOrAbsolute);
            transferRequest.DownloadLocation = downloadUri;
            transferRequest.Tag = downloadFile;
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
            try
            {
                BackgroundTransferService.Add(transferRequest);
                
            }
            catch (InvalidOperationException ex)
            {
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
            String s = ((TextBlock)sender).Tag as string;
            //TODO
            //Show Details of the Map-Requires XML-Dat for the Map
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

            //Get correponding URL for the map. These Url are used for the Creation of the backgroundtransfer when the finisched-Events of the mapInfo-Service trigger
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
                if (textBoxSearch.Text==null|| textBoxSearch.Text.Equals(String.Empty))
                    filteredMaps = maps;
                else
                {
                    filteredMaps = new List<String>();
                    foreach (String mapname in maps)
                    {
                        if (mapname.ToLower().Contains(textBoxSearch.Text.ToLower()))
                            filteredMaps.Add(mapname);
                    }
                }
            }
            URLListBox.ItemsSource = filteredMaps;
        }
        #endregion
    }
}