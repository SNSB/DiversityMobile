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
        private double initialScale;
        private double offsetFromCenterX = 0;
        private double offsetFromCenterY = 0;
       



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
            Line t1t2 = new Line(t1, new Vector(t1, t2));
            Point center = t1t2.MoveOnLineFromBaseForUnits(0.5);
            Point s1 = e.GetPosition(scrollViewer, 0);
            Point s2 = e.GetPosition(scrollViewer, 1);
            Line s1s2 = new Line(s1, new Vector(s1, s2));
            Point scrollCenter = s1s2.MoveOnLineFromBaseForUnits(0.5);
            //percTouchCenter = VM.calculatePixelToPercentPoint(center);            
            offsetFromCenterX =  scrollCenter.X;
            offsetFromCenterY =  scrollCenter.Y;
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
    }
}