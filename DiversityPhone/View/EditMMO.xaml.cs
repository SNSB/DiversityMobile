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
using DiversityPhone.ViewModels;
using DiversityPhone.Model;

namespace DiversityPhone.View
{
    public partial class EditMMO : PhoneApplicationPage
    {
        private EditMultimediaObjectVM VM { get { return DataContext as EditMultimediaObjectVM; } }

        public EditMMO()
        {
            InitializeComponent();
            if (VM != null)
            {
                MultimediaObject mmo = VM.Model;
                if (mmo.Uri == null || mmo.Uri == String.Empty)
                    textBlock1.Text = "NewItem";
                else
                    textBlock1.Text = mmo.Uri;

            }
        }
    }
}