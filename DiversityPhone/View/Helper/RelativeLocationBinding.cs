using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace DiversityPhone.View
{
    public sealed class RelativeLocationBinding : IDisposable
    {
        private Point? _RelativeLocation = null;
        public Point? RelativeLocation 
        {
            get { return _RelativeLocation; }
            set
            {
                if (_RelativeLocation != value)
                {
                    _RelativeLocation = value;
                    updateLocation();
                }
            }                
        }

        FrameworkElement item, canvas_item;
        CompositeDisposable subscription = new CompositeDisposable();

        public RelativeLocationBinding(FrameworkElement item, FrameworkElement canvas_item, IObservable<Point?> locations = null)
        {  
            if(item == null)
                throw new ArgumentNullException("item");   
            if(canvas_item == null)
                throw new ArgumentNullException("canvas_item");   

            this.item = item;
            item.Visibility = Visibility.Collapsed;

            this.canvas_item = canvas_item;
            canvas_item.SizeChanged += item_SizeChanged;
            this.subscription.Add(Disposable.Create(() => canvas_item.SizeChanged -= item_SizeChanged));

            if (locations != null)
                this.subscription.Add(locations.Subscribe(loc => RelativeLocation = loc));
            
        }

        void item_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            updateLocation();
        }

        public void updateLocation()
        {
            if (subscription.IsDisposed)
                return;

            var transform = canvas_item.RenderTransform ?? new TranslateTransform() { X = 0, Y = 0 };           

            var result = (RelativeLocation.HasValue) ?  transform.Transform(RelativeLocation.Value) as Point? : null;
            if (!result.HasValue)
                item.Visibility = System.Windows.Visibility.Collapsed;
            else
            {
                item.Visibility = System.Windows.Visibility.Visible;
                Canvas.SetLeft(item, (result.Value.X) - (item.ActualWidth / 2));
                Canvas.SetTop(item, (result.Value.Y) - (item.ActualHeight / 2));
            }
        }

        public void Dispose()
        {
            subscription.Dispose();
            item = canvas_item = null;
        }        
    }
}
