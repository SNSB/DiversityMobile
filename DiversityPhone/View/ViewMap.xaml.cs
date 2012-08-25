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

using Microsoft.Phone.Controls;
using DiversityPhone.Model;
using DiversityPhone.ViewModels;
using System.Windows.Media.Imaging;
using System.Collections.Specialized;
using DiversityPhone.Model.Geometry;

namespace DiversityPhone.View
{
    public partial class ViewMap : PhoneApplicationPage
    {

        private ViewMapVM VM { get { return this.DataContext as ViewMapVM; } }
        
        private Point percTouchCenter= new Point(0, 0);
       
       



        public ViewMap()
        {
            InitializeComponent();

        }

        private void focusOn(double x, double y)
        {
            scrollViewer.ScrollToHorizontalOffset(x);
            scrollViewer.ScrollToVerticalOffset(y);
        }

        #region OnPinch

        private void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            Point t1 = e.GetPosition(MainCanvas, 0);
            Point t2 = e.GetPosition(MainCanvas, 1);
            
            //percTouchCenter = VM.calculatePixelToPercentPoint(center); 
        }

        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            VM.Scale *= e.DistanceRatio;

            //Point center = VM.calculatePercentToPixelPoint(percTouchCenter, 0, 0, VM.Zoom);
            //focusOn(center.X - offsetFromCenterX, center.Y - offsetFromCenterY);
        }

        private void OnPinchCompleted(object sender, PinchGestureEventArgs e)
        {
        }

        #endregion

        #region Scroll

        private void scrollViewer_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        { 
            //if (VM != null)
            //    if(VM.ActualPosPoint.X>0 && VM.ActualPosPoint.Y>0)
            //            focusOn(VM.ActualPosPoint.X-scrollViewer.Width/2, VM.ActualPosPoint.Y-scrollViewer.Height/2);
        }

        private void scrollViewer_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            //if (VM != null && VM.ActualPosPoint != null)
            //    if (VM.ActualPosPoint.X > 0 && VM.ActualPosPoint.Y > 0)
            //        focusOn(VM.ActualPosPoint.X - scrollViewer.Width / 2, VM.ActualPosPoint.Y - scrollViewer.Height / 2);

        }

        #endregion

        private void SelectMap_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.SelectMap.Execute(null);
        }
    }
}