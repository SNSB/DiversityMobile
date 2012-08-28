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
    public class RelativeLocationBinding : IDisposable
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

        private Point _TargetSize = new Point(0,0);
        public Point TargetSize
        {
            get { return _TargetSize; }
            set
            {
                if (_TargetSize != value)
                {
                    _TargetSize = value;
                    updateLocation();
                }
            }
        }

        FrameworkElement item;        
        CompositeDisposable subscription = new CompositeDisposable();

        public RelativeLocationBinding(FrameworkElement item, IObservable<Point> container_sizes, IObservable<Point?> locations = null)
        {  
            if(item == null)
                throw new ArgumentNullException("item");
            if(container_sizes == null)
                throw new ArgumentNullException("container_sizes");

            this.item = item;
            item.Visibility = Visibility.Collapsed;

            


            this.subscription.Add(container_sizes.Subscribe(size => TargetSize = size));

            if (locations != null)
                this.subscription.Add(locations.Subscribe(loc => RelativeLocation = loc));
            
        }

        private void updateLocation()
        {     
            var result = (RelativeLocation.HasValue) ? new Point(TargetSize.X * RelativeLocation.Value.X, TargetSize.Y * RelativeLocation.Value.Y) as Point? : null;
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
        }
    }
}
