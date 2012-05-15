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
using DiversityPhone.ViewModels.Maps;
using System.Collections.Specialized;
using System.Windows.Media.Imaging;
using DiversityPhone.Model;

namespace DiversityPhone.View
{
    public partial class ViewMapES : PhoneApplicationPage
    {

        private ViewMapESVM VM { get { return this.DataContext as ViewMapESVM; } }
        private const double SCALEMIN = 0.2;
        private const double SCALEMAX = 3;
        private IList<Image> _seriesPointImages;


        public ViewMapES()
        {
            InitializeComponent();
            _seriesPointImages = new List<Image>();
            VM.SeriesPos.CollectionChanged += new NotifyCollectionChangedEventHandler(setSeriesPointsEventTriggered);
        }

        #region PointImages on Screen

        private void recalculateSeriesPoints()
        {
            if (_seriesPointImages == null)
                _seriesPointImages = new List<Image>();
            foreach (Image im in _seriesPointImages)
            {
                if (MainCanvas.Children.Contains(im))
                    MainCanvas.Children.Remove(im);
            }
            _seriesPointImages.Clear();
            foreach (Point p in VM.SeriesPerc)
            {
                setSeriesPointImage(p);
            }
        }

        private void addSeriesPointImage(Point percentPoint)
        {
            if (_seriesPointImages == null)
                _seriesPointImages = new List<Image>();
            setSeriesPointImage(percentPoint);
        }


        private void setSeriesPointImage(Point p)
        {
            Image im = new Image();
            Point pixel = VM.calculatePixelPointForSeriesPercPoint(p);
            if (pixel.X > 0 && pixel.Y > 0)
            {
                im.Source = new BitmapImage(new Uri("/Images/BlackPoint.png", UriKind.RelativeOrAbsolute));//Einmal als Source setzen
                MainCanvas.Children.Add(im);
                Canvas.SetTop(im, pixel.Y);
                Canvas.SetLeft(im, pixel.X);
                _seriesPointImages.Add(im);
            }
        }

        #endregion

        private void focusOn(double x, double y)
        {
            scrollViewer.ScrollToHorizontalOffset(x);
            scrollViewer.ScrollToVerticalOffset(y);
        }

        //New GPS-Value is coming for an Eventseries
        private void setSeriesPointsEventTriggered(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Object o in e.NewItems)
                    try
                    {
                        GeoPointForSeries gp = (GeoPointForSeries)o;
                        Point? p = VM.calculateGPSToPercentagePoint(gp.Latitude, gp.Longitude);
                        if (p != null)
                        {
                            VM.SeriesPerc.Add((Point)p);
                            addSeriesPointImage((Point)p);
                        }
                    }
                    catch (Exception ex) { };
            }
        }

        #region OnPinch

        private void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            VM.Zoom = transform.ScaleX;
        }

        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            double scale = VM.Zoom * Math.Sqrt(e.DistanceRatio);
            if (scale < SCALEMIN)
                scale = SCALEMIN;
            if (scale > SCALEMAX)
                scale = SCALEMAX;
            VM.Zoom = scale;
            transform.ScaleX = scale;
            transform.ScaleY = scale;
            MainCanvas.Height = VM.BaseHeight * VM.Zoom;
            MainCanvas.Width = VM.BaseWidth * VM.Zoom;
            if (VM.EventSeries != null)
                recalculateSeriesPoints();
        }
        #endregion

        #region Scroll

        private void scrollViewer_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (VM != null)
                if (VM.ActualPosPoint.X > 0 && VM.ActualPosPoint.Y > 0)
                    focusOn(VM.ActualPosPoint.X - scrollViewer.Width / 2, VM.ActualPosPoint.Y - scrollViewer.Height / 2);
        }

        private void scrollViewer_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            double scale = VM.Zoom * Math.Sqrt(2);
            if (scale < SCALEMIN)
                scale = SCALEMIN;
            if (scale > SCALEMAX)
                scale = SCALEMAX;
            VM.Zoom = scale;
            transform.ScaleX = scale;
            transform.ScaleY = scale;
            MainCanvas.Height = VM.BaseHeight * VM.Zoom;
            MainCanvas.Width = VM.BaseWidth * VM.Zoom;
            recalculateSeriesPoints();
        }

        #endregion


        

    }
}