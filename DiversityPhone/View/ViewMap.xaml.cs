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
using System.Windows.Media.Imaging;
using System.Collections.Specialized;

namespace DiversityPhone.View
{
    public partial class ViewMap : PhoneApplicationPage
    {

        private ViewMapVM VM { get { return this.DataContext as ViewMapVM; } }
        private const double SCALEMIN = 0.2;
        private const double SCALEMAX = 3;
        private IList<Image> _seriesPointImages;


        public ViewMap()
        {
            InitializeComponent();
            _seriesPointImages = new List<Image>();
            VM.SeriesPos.CollectionChanged += new NotifyCollectionChangedEventHandler(setSeriesPointsEventTriggered);
        }

        private void focusOn(double x, double y)
        {
            scrollViewer.ScrollToHorizontalOffset(x);
            scrollViewer.ScrollToVerticalOffset(y);
        }


        private void setSeriesPoint(GeoPointForSeries gp)
        {
        }


        #region Points in Pixel

        #endregion

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
            foreach(GeoPointForSeries gp in VM.SeriesPos)
            {
                setSeriesPointImage(gp);
            }
        }

        private void addSeriesPointImage(GeoPointForSeries gp)
        {
            if (_seriesPointImages == null)
                _seriesPointImages = new List<Image>();
            setSeriesPointImage(gp);
        }


        private void setSeriesPointImage(GeoPointForSeries gp)
        {
            Image im = new Image();
            Point p = VM.calculatePixelPointForSeriesPoint(gp);
            if(p.X>0 && p.Y>0)
            {
                im.Source = new BitmapImage(new Uri("/Images/BlackPoint.png", UriKind.RelativeOrAbsolute));//Einmal als Souce setzen
                MainCanvas.Children.Add(im);
                Canvas.SetTop(im, p.Y);
                Canvas.SetLeft(im, p.X);
                _seriesPointImages.Add(im);
            }
        }

        #endregion

        private void setSeriesPointsEventTriggered(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach(Object o in e.NewItems)
                try
                {
                    GeoPointForSeries gp = (GeoPointForSeries)o;
                    addSeriesPointImage(gp);
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
            MainCanvas.Width  = VM.BaseWidth * VM.Zoom;
        }

        private void OnPinchCompleted(object sender, PinchGestureEventArgs e)
        {
            VM.calculatePixelPointForActual();
            if (VM.Event != null || VM.IU != null)
                VM.calculatePixelPointForItem();
            if (VM.EventSeries != null)
                recalculateSeriesPoints();          
        }

        #endregion

        private void scrollViewer_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            
            if (VM != null && VM.ItemPosPoint != null)
                if(VM.ActualPosPoint.X>0 && VM.ActualPosPoint.Y>0)
                     focusOn(VM.ActualPosPoint.X-scrollViewer.Width/2, VM.ActualPosPoint.Y-scrollViewer.Height/2);
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
            VM.calculatePixelPointForActual();
            recalculateSeriesPoints();
            //if(VM!= null && VM.ItemPosPoint!=null)
            //    if (VM.ItemPosPoint.X > 0 && VM.ItemPosPoint.Y > 0)
            //        focusOn(VM.ItemPosPoint.X-scrollViewer.Width/2, VM.ItemPosPoint.Y-scrollViewer.Height/2);
            
        }
    }
}