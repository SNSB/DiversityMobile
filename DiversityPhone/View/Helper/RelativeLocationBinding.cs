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

namespace DiversityPhone.View
{
    public class RelativeLocationBinding : IDisposable
    {        
        FrameworkElement item;        
        IDisposable subscription;

        public RelativeLocationBinding(FrameworkElement item,IObservable<Point> container_sizes, IObservable<Point?> locations)
        {   
            this.item = item;
            item.Visibility = Visibility.Collapsed;
            this.subscription = Observable.CombineLatest(
                container_sizes,
                locations,
                (size, loc) => (loc.HasValue) ? new Point(size.X * loc.Value.X, size.Y * loc.Value.Y) as Point? : null)
                .Subscribe(updateLocation);
            
        }

        private void updateLocation(Point? loc)
        {            
            if (!loc.HasValue)
                item.Visibility = System.Windows.Visibility.Collapsed;
            else
            {
                item.Visibility = System.Windows.Visibility.Visible;
                Canvas.SetLeft(item, (loc.Value.X ) - (item.ActualWidth / 2));
                Canvas.SetTop(item, (loc.Value.Y ) - (item.ActualHeight / 2));
            }
        }

        public void Dispose()
        {
            subscription.Dispose();
        }
    }
}
