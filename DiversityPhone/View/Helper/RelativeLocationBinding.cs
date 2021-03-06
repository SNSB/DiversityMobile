﻿using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DiversityPhone.View
{
    public sealed class RelativeLocationBinding : IDisposable
    {
        private Point? _RelativeLocation = null;

        public Point? RelativeLocation
        {
            get
            {
                return _RelativeLocation;
            }
            set
            {
                if (_RelativeLocation != value)
                {
                    if (subscription.IsDisposed)
                        return;
                    _RelativeLocation = value;
                    updateLocation();
                }
            }
        }

        private Transform _Transform;

        public Transform Transform
        {
            get
            {
                return _Transform;
            }
            set
            {
                _Transform = value;
                updateLocation();
            }
        }

        private FrameworkElement item;

        private CompositeDisposable subscription = new CompositeDisposable();

        public RelativeLocationBinding(FrameworkElement item, IObservable<Transform> transforms, IObservable<Point?> locations = null)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (transforms == null)
                throw new ArgumentNullException("transforms");

            this.item = item;
            item.Visibility = Visibility.Collapsed;

            subscription.Add(transforms.Subscribe(t => Transform = t));

            if (locations != null)
                this.subscription.Add(locations.Subscribe(loc => RelativeLocation = loc));
        }

        private void item_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            updateLocation();
        }

        private void updateLocation()
        {
            if (subscription.IsDisposed)
                return;

            var result = (RelativeLocation.HasValue && Transform != null) ? Transform.Transform(RelativeLocation.Value) as Point? : null;
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
            item = null;
        }
    }
}