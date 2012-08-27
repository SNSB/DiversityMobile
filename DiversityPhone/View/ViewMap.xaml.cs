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
using DiversityPhone.View.Appbar;
using System.Reactive.Subjects;

namespace DiversityPhone.View
{
    public partial class ViewMap : PhoneApplicationPage
    {

        private ViewMapVM VM { get { return this.DataContext as ViewMapVM; } }

        private BehaviorSubject<double> scale_subject = new BehaviorSubject<double>(1.0f);
        private double _CurrentScale = 1.0f;
        public double CurrentScale 
        {
            get { return _CurrentScale; }
            set
            {
                if (value < 0.2f)
                    value = 0.2f;
                else if (value > 3.0f)
                    value = 3.0f;

                if (_CurrentScale != value)
                {
                    _CurrentScale = value;
                    scale_subject.OnNext(_CurrentScale);
                }
            }
        }
        

        private EditPageSaveEditButton _btn;
       
       



        public ViewMap()
        {
            InitializeComponent();

            _btn = new EditPageSaveEditButton(this.ApplicationBar, VM);
        }

        private void focusOn(double x, double y)
        {
            scrollViewer.ScrollToHorizontalOffset(x);
            scrollViewer.ScrollToVerticalOffset(y);
        }

        private void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            
        }

        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            CurrentScale *= e.DistanceRatio;
        }

        private void OnPinchCompleted(object sender, PinchGestureEventArgs e)
        {
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
            scale_subject
                .Subscribe(scale => mapImg.RenderTransform = new ScaleTransform() { ScaleY = scale, ScaleX = scale})
                );

            s.Add(new RelativeLocationBinding(MainCanvas, currentPosImg, VM.ObservableForProperty(x => x.CurrentLocation).Value().StartWith(VM.CurrentLocation)));

            s.Add(new RelativeLocationBinding(MainCanvas, currentLocalizationImg, VM.ObservableForProperty(x => x.PrimaryLocalization).Value().StartWith(VM.PrimaryLocalization)));

            subscriptions = s;
        }

        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            subscriptions.Dispose();
        }

        private void MainCanvas_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var point = e.GetPosition(MainCanvas);
            point.X /= MainCanvas.Width;
            point.Y /= MainCanvas.Height;
            if(VM != null && VM.SetLocation.CanExecute(point))
                VM.SetLocation.Execute(point);
        }
    }
}