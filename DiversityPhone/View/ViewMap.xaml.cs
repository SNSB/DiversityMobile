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
using System.Windows.Media;

namespace DiversityPhone.View
{
    public partial class ViewMap : PhoneApplicationPage
    {

        private ViewMapVM VM { get { return this.DataContext as ViewMapVM; } }

        private EditPageSaveEditButton _btn;
        private RelativeLocationBinding _currentLoc, _currentPos;

        // these two fully define the zoom state:
        private double TotalImageScale = 1d;
        private Point ImagePosition = new Point(0, 0);

        private Point _oldFinger1;
        private Point _oldFinger2;
        private double _oldScaleFactor;

        private void OnPinchStarted(object s, PinchStartedGestureEventArgs e)
        {
            _oldFinger1 = e.GetPosition(mapImg, 0);
            _oldFinger2 = e.GetPosition(mapImg, 1);
            _oldScaleFactor = 1;

            currentLocalizationImg.Visibility = Visibility.Collapsed;
            currentPosImg.Visibility = Visibility.Collapsed;
        }

        private void OnPinchCompleted(object s, PinchGestureEventArgs args)
        {
            if (_currentLoc != null)
                _currentLoc.updateLocation();
            if (_currentPos != null)
                _currentPos.updateLocation();
        }

        private void OnPinchDelta(object s, PinchGestureEventArgs e)
        {
            var scaleFactor = e.DistanceRatio / _oldScaleFactor;

            if (scaleFactor > 3.0f || scaleFactor < 0.5f) 
                return;

            var currentFinger1 = e.GetPosition(mapImg, 0);
            var currentFinger2 = e.GetPosition(mapImg, 1);

            var translationDelta = GetTranslationDelta(
                currentFinger1,
                currentFinger2,
                _oldFinger1,
                _oldFinger2,
                ImagePosition,
                scaleFactor);

            _oldFinger1 = currentFinger1;
            _oldFinger2 = currentFinger2;
            _oldScaleFactor = e.DistanceRatio;

            UpdateImage(scaleFactor, translationDelta);
        }

        private void UpdateImage(double scaleFactor, Point delta)
        {
            TotalImageScale *= scaleFactor;
            ImagePosition = new Point(ImagePosition.X + delta.X, ImagePosition.Y + delta.Y);

            var transform = (CompositeTransform)mapImg.RenderTransform;
            transform.ScaleX = TotalImageScale;
            transform.ScaleY = TotalImageScale;
            transform.TranslateX = ImagePosition.X;
            transform.TranslateY = ImagePosition.Y;
        }

        private Point GetTranslationDelta(
            Point currentFinger1, Point currentFinger2,
            Point oldFinger1, Point oldFinger2,
            Point currentPosition, double scaleFactor)
        {
            var newPos1 = new Point(
                currentFinger1.X + (currentPosition.X - oldFinger1.X) * scaleFactor,
                currentFinger1.Y + (currentPosition.Y - oldFinger1.Y) * scaleFactor);

            var newPos2 = new Point(
                currentFinger2.X + (currentPosition.X - oldFinger2.X) * scaleFactor,
                currentFinger2.Y + (currentPosition.Y - oldFinger2.Y) * scaleFactor);

            var newPos = new Point(
                (newPos1.X + newPos2.X) / 2,
                (newPos1.Y + newPos2.Y) / 2);

            return new Point(
                newPos.X - currentPosition.X,
                newPos.Y - currentPosition.Y);
        }
        
        public ViewMap()
        {
            InitializeComponent();

            _btn = new EditPageSaveEditButton(this.ApplicationBar, VM);
            mapImg.RenderTransform = new CompositeTransform();
        }

        private void SelectMap_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.SelectMap.Execute(null);
        }

        IDisposable subscriptions = Disposable.Empty;
        CompositeDisposable additionallocalization_images;

        private IObservable<Point?> ScaleToImage(IObservable<Point?> points, IObservable<BitmapImage> images)
        {
            return images.CombineLatest(points,
                (img, p) =>
                {
                    if (p.HasValue && img != null)
                        return new Point() { X = p.Value.X * img.PixelWidth, Y = p.Value.Y * img.PixelHeight } as Point?;
                    else
                        return null;
                });     
        }


        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            additionallocalization_images = new CompositeDisposable(clear_additional_locs());

            var s = new CompositeDisposable(additionallocalization_images as IDisposable);

            var image_obs = VM.ObservableForProperty(x => x.MapImage).Value().StartWith(VM.MapImage).Where(img => img != null);        

            
            
            s.Add(image_obs.Subscribe(img => mapImg.Source = img));


            _currentPos = new RelativeLocationBinding(currentPosImg, mapImg, ScaleToImage(VM.ObservableForProperty(x => x.CurrentLocation).Value().StartWith(VM.CurrentLocation), image_obs));
            s.Add(_currentPos);

            _currentLoc = new RelativeLocationBinding(currentLocalizationImg, mapImg, ScaleToImage( VM.ObservableForProperty(x => x.PrimaryLocalization).Value().StartWith(VM.PrimaryLocalization), image_obs));
            s.Add(_currentLoc);

            s.Add(VM.AdditionalLocalizations.ToObservable()
                .Merge(VM.AdditionalLocalizations.ItemsAdded)
                .Subscribe(it =>
                {
                    var source = this.Resources["GPSPointImage"] as BitmapImage;
                    if(source != null)
                    {
                        var img = new Image() { Source = source, Height = source.PixelHeight, Width = source.PixelWidth };
                        var binding = new RelativeLocationBinding(img, mapImg) { RelativeLocation = it };
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

        private void MapGrid_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            double x = (Canvas.GetLeft(currentPosImg) + currentPosImg.ActualWidth / 2) - scrollViewer.ViewportWidth / 2;
            double y = (Canvas.GetTop(currentPosImg) + currentPosImg.ActualHeight / 2) - scrollViewer.ViewportHeight / 2;

            scrollViewer.ScrollToHorizontalOffset(x);
            scrollViewer.ScrollToVerticalOffset(y);
        }
    }
}