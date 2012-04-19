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
using DiversityPhone.Model;
using DiversityPhone.ViewModels;

namespace DiversityPhone.View
{
    public partial class ViewMap : PhoneApplicationPage
    {

        private ViewMapVM VM { get { return this.DataContext as ViewMapVM; } }

        public ViewMap()
        {
            InitializeComponent();
        }

        private void focusOn(int x, int y)
        {
            scrollViewer.ScrollToHorizontalOffset(x);
            scrollViewer.ScrollToVerticalOffset(y);
        }


        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.Map != null && VM.MapImage!=null)
            {
                VM.Zoom = (slider1.Value * slider1.Value);
                this.Map.Height = (VM.MapImage.PixelHeight) / VM.Zoom;
                this.Map.Width = (VM.MapImage.PixelWidth) / VM.Zoom;
                VM.calculatePixelPointForActual();
                VM.calculatePixelPointForItem();
            }
        }

      
        private void scrollViewer_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            focusOn(0, 0);
        }
    }
}