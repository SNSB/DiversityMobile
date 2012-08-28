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

        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            CurrentScale *= e.DistanceRatio;
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
            var s = new CompositeDisposable();

            var image_obs = VM.ObservableForProperty(x => x.MapImage).Value().StartWith(VM.MapImage).Where(img => img != null);                    
                    

            var size_obs = Observable.CombineLatest(
                    scale_subject,
                    image_obs,
                    (scale, img) => new Point() { X = scale * img.PixelWidth, Y = scale * img.PixelHeight});
            
            s.Add(image_obs.Subscribe(img => mapImg.Source = img));

            s.Add(size_obs
                    .Subscribe(size => { mapImg.Height = size.Y; mapImg.Width = size.X; })
                );
            s.Add(size_obs
                    .Subscribe(size => { MainCanvas.Height = size.Y; MainCanvas.Width = size.X; })
                );

            s.Add(new RelativeLocationBinding(currentPosImg, size_obs, VM.ObservableForProperty(x => x.CurrentLocation).Value().StartWith(VM.CurrentLocation)));

            s.Add(new RelativeLocationBinding(currentLocalizationImg, size_obs, VM.ObservableForProperty(x => x.PrimaryLocalization).Value().StartWith(VM.PrimaryLocalization)));

            s.Add(VM.AdditionalLocalizations.ItemsAdded.Subscribe(it =>
                {
                    var img = new Image() { Source = this.Resources["GPSPointImage"] as BitmapImage };
                    var binding = new RelativeLocationBinding(img, size_obs) { RelativeLocation = it };
                    additionallocalization_images.Add(binding);
                    MainCanvas.Children.Add(img);
                }));

            s.Add(VM.AdditionalLocalizations.IsEmpty.DistinctUntilChanged().Where(empty => empty).Subscribe(_ => additionallocalization_images.Dispose()));

            subscriptions = s;
        }

        private IDisposable clear_additional_locs()
        {
            return Disposable.Create(() =>
                {                    
                    MainCanvas.Children.Clear();
                    MainCanvas.Children.Add(mapImg);
                    MainCanvas.Children.Add(currentPosImg);
                    MainCanvas.Children.Add(currentLocalizationImg);
                    additionallocalization_images = new CompositeDisposable(clear_additional_locs());
                });
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