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
using ReactiveUI;
using System.Reactive.Linq;
using Microsoft.Phone.Controls;
using DiversityPhone.Model;
using DiversityPhone.ViewModels;
using System.Windows.Media.Imaging;
using System.Collections.Specialized;
using DiversityPhone.Model.Geometry;
using System.Reactive.Disposables;
using System.Reactive;

namespace DiversityPhone.View
{
    public partial class ViewMap : PhoneApplicationPage
    {

        private ViewMapVM VM { get { return this.DataContext as ViewMapVM; } }

        private double currentScale = 1.0, centerX = 0.5, centerY = 0.5;
        
       
       



        public ViewMap()
        {
            InitializeComponent();
        }

        private void focusOn(double x, double y)
        {
            scrollViewer.ScrollToHorizontalOffset(x);
            scrollViewer.ScrollToVerticalOffset(y);
        }

        private void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            Point t1 = e.GetPosition(MainCanvas, 0);
            Point t2 = e.GetPosition(MainCanvas, 1);

            

            currentScale = VM.Scale;
        }

        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            VM.Scale = currentScale * e.DistanceRatio;
        }

        private void OnPinchCompleted(object sender, PinchGestureEventArgs e)
        {
        }

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

        private void SelectMap_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.SelectMap.Execute(null);
        }

        IDisposable subscriptions = Disposable.Empty;

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            var s = new CompositeDisposable();
            s.Add(
            VM.ObservableForProperty(x => x.Scale).Value()
                .Subscribe(scale => MainCanvas.RenderTransform = new ScaleTransform() { ScaleY = scale, ScaleX = scale, CenterX = centerX, CenterY = centerY })
                );

            s.Add(new RelativeLocationBinding(MainCanvas, currentPosImg, VM.ObservableForProperty(x => x.CurrentLocation).Value().StartWith(VM.CurrentLocation)));

            s.Add(new RelativeLocationBinding(MainCanvas, currentLocalizationImg, VM.ObservableForProperty(x => x.CurrentLocalization).Value().StartWith(VM.CurrentLocalization)));

            subscriptions = s;
        }

        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            subscriptions.Dispose();
        }
    }
}