using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DiversityPhone.View
{
    public partial class ViewMap : PhoneApplicationPage
    {

        private ViewMapVM VM { get { return this.DataContext as ViewMapVM; } }

        private EditPageSaveEditButton _btn;
        private RelativeLocationBinding _currentLoc, _currentPos;

        private ISubject<Transform> transform_subject = new Subject<Transform>();
        private IObservable<Transform> update_transform;


        // these two fields fully define the zoom state:
        private double TotalImageScale
        {
            get { return VM.ImageScale; }
            set { VM.ImageScale = value; }
        }
        private Point ImagePosition
        {
            get { return VM.ImageOffset; }
            set { VM.ImageOffset = value; }
        }


        private const double MAX_IMAGE_ZOOM = 10;
        private Point _oldFinger1;
        private Point _oldFinger2;
        private double _oldScaleFactor;


        #region Event handlers

        /// <summary>
        /// Initializes the zooming operation
        /// </summary>
        private void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            _oldFinger1 = e.GetPosition(ImgZoom, 0);
            _oldFinger2 = e.GetPosition(ImgZoom, 1);
            _oldScaleFactor = 1;
        }

        private void OnPinchCompleted(object s, PinchGestureEventArgs args)
        {
            
        }

        /// <summary>
        /// Computes the scaling and translation to correctly zoom around your fingers.
        /// </summary>
        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            var scaleFactor = e.DistanceRatio / _oldScaleFactor;
            if (!IsScaleValid(scaleFactor))
                return;

            var currentFinger1 = e.GetPosition(ImgZoom, 0);
            var currentFinger2 = e.GetPosition(ImgZoom, 1);

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

            UpdateImageScale(scaleFactor);
            UpdateImagePosition(translationDelta);
        }

        /// <summary>
        /// Moves the image around following your finger.
        /// </summary>
        private void OnDragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            var translationDelta = new Point(e.HorizontalChange, e.VerticalChange);

            if (IsDragValid(1, translationDelta))
                UpdateImagePosition(translationDelta);
        }

        /// <summary>
        /// Resets the image scaling and position
        /// </summary>
        private void OnDoubleTap(object sender, GestureEventArgs e)
        {
            ResetImagePosition();
        }

        #endregion

        #region Utils

        /// <summary>
        /// Computes the translation needed to keep the image centered between your fingers.
        /// </summary>
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

        /// <summary>
        /// Updates the scaling factor by multiplying the delta.
        /// </summary>
        private void UpdateImageScale(double scaleFactor)
        {
            TotalImageScale *= scaleFactor;
            ApplyScale();
        }

        /// <summary>
        /// Applies the computed scale to the image control.
        /// </summary>
        private void ApplyScale()
        {
            ((CompositeTransform)ImgZoom.RenderTransform).ScaleX = TotalImageScale;
            ((CompositeTransform)ImgZoom.RenderTransform).ScaleY = TotalImageScale;
            transform_subject.OnNext(ImgZoom.RenderTransform);
        }

        /// <summary>
        /// Updates the image position by applying the delta.
        /// Checks that the image does not leave empty space around its edges.
        /// </summary>
        private void UpdateImagePosition(Point delta)
        {
            var newPosition = new Point(ImagePosition.X + delta.X, ImagePosition.Y + delta.Y);

            if (newPosition.X > 0) newPosition.X = 0;
            if (newPosition.Y > 0) newPosition.Y = 0;

            if ((ImgZoom.ActualWidth * TotalImageScale) + newPosition.X < ImgZoom.ActualWidth)
                newPosition.X = ImgZoom.ActualWidth - (ImgZoom.ActualWidth * TotalImageScale);

            if ((ImgZoom.ActualHeight * TotalImageScale) + newPosition.Y < ImgZoom.ActualHeight)
                newPosition.Y = ImgZoom.ActualHeight - (ImgZoom.ActualHeight * TotalImageScale);

            ImagePosition = newPosition;

            ApplyPosition();
        }

        /// <summary>
        /// Applies the computed position to the image control.
        /// </summary>
        private void ApplyPosition()
        {
            ((CompositeTransform)ImgZoom.RenderTransform).TranslateX = ImagePosition.X;
            ((CompositeTransform)ImgZoom.RenderTransform).TranslateY = ImagePosition.Y;
            transform_subject.OnNext(ImgZoom.RenderTransform);
        }

        /// <summary>
        /// Resets the zoom to its original scale and position
        /// </summary>
        private void ResetImagePosition()
        {
            TotalImageScale = 1;
            ImagePosition = new Point(0, 0);
            ApplyScale();
            ApplyPosition();
        }

        /// <summary>
        /// Checks that dragging by the given amount won't result in empty space around the image
        /// </summary>
        private bool IsDragValid(double scaleDelta, Point translateDelta)
        {
            if (ImagePosition.X + translateDelta.X > 0 || ImagePosition.Y + translateDelta.Y > 0)
                return false;

            if (((ImgZoom.ActualWidth * TotalImageScale * scaleDelta) + (ImagePosition.X + translateDelta.X) < ImgZoom.ActualWidth) &&
                ((ImgZoom.ActualHeight * TotalImageScale * scaleDelta) + (ImagePosition.Y + translateDelta.Y) < ImgZoom.ActualHeight))
                return false;

            return true;
        }

        /// <summary>
        /// Tells if the scaling is inside the desired range
        /// </summary>
        private bool IsScaleValid(double scaleDelta)
        {
            return (TotalImageScale * scaleDelta >= 1) && (TotalImageScale * scaleDelta <= MAX_IMAGE_ZOOM);
        }

        #endregion     

       
        public ViewMap()
        {
            InitializeComponent();

            _btn = new EditPageSaveEditButton(this.ApplicationBar, VM);
            _btn.Button.Click += (s, args) => { this.ApplicationBar.Mode = (this.ApplicationBar.Mode == ApplicationBarMode.Minimized) ? ApplicationBarMode.Default : ApplicationBarMode.Minimized; };
            ImgZoom.RenderTransform = new CompositeTransform() { CenterX = 0, CenterY = 0 };

            var transforms = transform_subject.Replay(1);
            transforms.Connect();
            update_transform = transforms;

            transform_subject.OnNext(ImgZoom.RenderTransform);
        }

        private void SelectMap_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.SelectMap.Execute(null);
        }

        IDisposable subscriptions = Disposable.Empty;
        CompositeDisposable additionallocalization_images;

        private Point? ScaleToImage(Point? p)
        {
            if (p.HasValue)
                return new Point() { X = p.Value.X * ImgZoom.ActualWidth, Y = p.Value.Y * ImgZoom.ActualHeight } as Point?;
            else
                return null;                   
        }


        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            additionallocalization_images = new CompositeDisposable(clear_additional_locs());

            var s = new CompositeDisposable(additionallocalization_images as IDisposable);

            var image_obs = VM.ObservableForProperty(x => x.MapImage).Value()              
                .StartWith(VM.MapImage).Where(img => img != null);



            s.Add(image_obs.Subscribe(img => { ImgZoom.Source = img; }));


            _currentPos = new RelativeLocationBinding(currentPosImg, update_transform, VM.ObservableForProperty(x => x.CurrentLocation).Value().StartWith(VM.CurrentLocation).Select(ScaleToImage));
            s.Add(_currentPos);

            _currentLoc = new RelativeLocationBinding(currentLocalizationImg, update_transform, VM.ObservableForProperty(x => x.PrimaryLocalization).Value().StartWith(VM.PrimaryLocalization).Select(ScaleToImage));
            s.Add(_currentLoc);

            s.Add(
                VM.AdditionalLocalizations
                .ObserveOnDispatcher()
                .Do(_ => additionallocalization_images.Dispose())
                .SelectMany(p => p)
                .ObserveOnDispatcher()
                .Subscribe(p =>
                    {                        
                        var it = ScaleToImage(p);
                        var source = this.Resources["GPSPointImage"] as BitmapImage;
                        if(source != null)
                        {
                            var img = new Image() { Source = source, Height = source.PixelHeight, Width = source.PixelWidth };
                            var binding = new RelativeLocationBinding(img, update_transform) { RelativeLocation = it };
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
            var point = e.GetPosition(ImgZoom);
            point.X /= ImgZoom.ActualWidth;
            point.Y /= ImgZoom.ActualHeight;
            if(VM != null && VM.SetLocation.CanExecute(point))
                VM.SetLocation.Execute(point);
        }

        private void MapGrid_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            double x = (Canvas.GetLeft(currentPosImg) + currentPosImg.ActualWidth / 2);
            double y = (Canvas.GetTop(currentPosImg) + currentPosImg.ActualHeight / 2);

            
        }
    }
}