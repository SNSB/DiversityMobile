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

namespace DiversityPhone.View
{
    public class RelativeLocationBinding : IDisposable
    {
        Canvas container;
        FrameworkElement item;        
        IDisposable subscription;

        public RelativeLocationBinding(Canvas container, FrameworkElement item, IObservable<Point?> locations)
        {
            this.container = container;
            this.item = item;
            item.Visibility = Visibility.Collapsed;
            this.subscription = locations.Subscribe(updateLocation);

            
        }

        private void updateLocation(Point? loc)
        {            
            if (!loc.HasValue)
                item.Visibility = System.Windows.Visibility.Collapsed;
            else
            {
                item.Visibility = System.Windows.Visibility.Visible;
                Canvas.SetLeft(item, (loc.Value.X * container.ActualWidth) - (item.ActualWidth / 2));
                Canvas.SetTop(item, (loc.Value.Y * container.ActualHeight) - (item.ActualHeight / 2));
            }
        }

        public void Dispose()
        {
            subscription.Dispose();
        }
    }
}
