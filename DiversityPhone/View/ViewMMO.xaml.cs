using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using DiversityPhone.ViewModels;
using System.IO.IsolatedStorage;
using System.IO;

namespace DiversityPhone.View
{
    public partial class ViewMMO : PhoneApplicationPage
    {

        private EditMultimediaObjectVM VM { get { return DataContext as EditMultimediaObjectVM; } }
        private BitmapImage bi;

        public ViewMMO()
        {
            InitializeComponent();
        }



        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            // The image will be read from isolated storage into the following byte array

            byte[] data;
            // Read the entire image in one go into a byte array
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {

                // Open the file - error handling omitted for brevity

                // Note: If the image does not exist in isolated storage the following exception will be generated:

                // System.IO.IsolatedStorage.IsolatedStorageException was unhandled

                // Message=Operation not permitted on IsolatedStorageFileStream

                using (IsolatedStorageFileStream isfs = isf.OpenFile(VM.Model.Uri, FileMode.Open, FileAccess.Read))
                {

                    // Allocate an array large enough for the entire file

                    data = new byte[isfs.Length];



                    // Read the entire file and then close it

                    isfs.Read(data, 0, data.Length);

                    isfs.Close();

                }

            }



            // Create memory stream and bitmap

            MemoryStream ms = new MemoryStream(data);
            bi = new BitmapImage();
            bi.SetSource(ms);
            PhotoImage.Source = bi;
        }

        private void ApplicationBarIconButton2_Click(object sender, EventArgs e)
        {
            PhotoImage.Source = VM.SavedImage;
        }


    }
}