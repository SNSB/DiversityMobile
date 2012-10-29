using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using ReactiveUI;
using Microsoft.Phone.Shell;

namespace DiversityPhone.View
{
    public partial class ViewMap : PhoneApplicationPage
    {

        private ViewMapVM VM { get { return this.DataContext as ViewMapVM; } }

        private Point touchcenteroffsets;

        private Point absoluteOffsets = new Point(0, 0);

        private double _PreviousScale = 1.0;
        private BehaviorSubject<double> scale_subject = new BehaviorSubject<double>(1.0);
        private double _CurrentScale = 1.0;
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

        private void SelectMap_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.SelectMap.Execute(null);
        }

        IDisposable subscriptions = Disposable.Empty;
        CompositeDisposable additionallocalization_images;


        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            additionallocalization_images = new CompositeDisposable(clear_additional_locs());

            var s = new CompositeDisposable(additionallocalization_images as IDisposable);

            var image_obs = VM.ObservableForProperty(x => x.MapImage).Value().StartWith(VM.MapImage).Where(img => img != null);     
                    

            var size_obs = Observable.CombineLatest(
                    scale_subject,
                    image_obs,
                    (scale, img) => new Point() { X = scale * img.PixelWidth, Y = scale * img.PixelHeight})
                    .Replay(1);
            s.Add(size_obs.Connect());

            s.Add(size_obs.Subscribe(size => { MapGrid.Width = size.X; MapGrid.Height = size.Y; }));
            
            s.Add(image_obs.Subscribe(img => mapImg.Source = img));           
           
            s.Add(new RelativeLocationBinding(currentPosImg, size_obs, VM.ObservableForProperty(x => x.CurrentLocation).Value().StartWith(VM.CurrentLocation)));

            s.Add(new RelativeLocationBinding(currentLocalizationImg, size_obs, VM.ObservableForProperty(x => x.PrimaryLocalization).Value().StartWith(VM.PrimaryLocalization)));

            s.Add(VM.AdditionalLocalizations.ToObservable()
                .Merge(VM.AdditionalLocalizations.ItemsAdded)
                .Subscribe(it =>
                {
                    var source = this.Resources["GPSPointImage"] as BitmapImage;
                    if(source != null)
                    {
                        var img = new Image() { Source = source, Height = source.PixelHeight, Width = source.PixelWidth };
                        var binding = new RelativeLocationBinding(img, size_obs) { RelativeLocation = it };
                        additionallocalization_images.Add(binding);
                        MainCanvas.Children.Add(img);
                    }
                }));

            subscriptions = s;
        }

        private IDisposable clear_additional_locs()
        {
            return Disposable.Create(() =>
                {                    
                    MainCanvas.Children.Clear();                    
                    MainCanvas.Children.Add(currentPosImg);
                    MainCanvas.Children.Add(currentLocalizationImg);
                    additionallocalization_images = new CompositeDisposable(clear_additional_locs());
                });
        }

        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            subscriptions.Dispose();
        }

        private void MapGrid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var point = e.GetPosition(mapImg);
            point.X /= mapImg.ActualWidth;
            point.Y /= mapImg.ActualHeight;
            if(VM != null && VM.SetLocation.CanExecute(point))
                VM.SetLocation.Execute(point);
        }

        private void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
        {            
            _PreviousScale = CurrentScale;

            Point t1 = e.GetPosition(mapImg, 0);
            Point t2 = e.GetPosition(mapImg, 1);

            absoluteOffsets = new Point(
                (t1.X + t2.X) / (2 * CurrentScale),
                (t1.Y + t2.Y) / (2 * CurrentScale));

            Point s1 = e.GetPosition(scrollViewer, 0);
            Point s2 = e.GetPosition(scrollViewer, 1);
            touchcenteroffsets = new Point(
                (s1.X + s2.X) / 2,
                (s1.Y + s2.Y) / 2);         
        }

        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            var scale = _PreviousScale * e.DistanceRatio;

            mapTransform.ScaleX = scale;
            mapTransform.ScaleY = scale;

            CurrentScale = scale;

            Point center = new Point(absoluteOffsets.X * CurrentScale, absoluteOffsets.Y * CurrentScale);
            focusOn(center.X - this.touchcenteroffsets.X, center.Y - touchcenteroffsets.Y);
        }

        private void MapGrid_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            double x = (Canvas.GetLeft(currentPosImg) + currentPosImg.ActualWidth / 2) - scrollViewer.ViewportWidth / 2;
            double y = (Canvas.GetTop(currentPosImg) + currentPosImg.ActualHeight / 2) - scrollViewer.ViewportHeight / 2;

            scrollViewer.ScrollToHorizontalOffset(x);
            scrollViewer.ScrollToVerticalOffset(y);
        }
    }
}