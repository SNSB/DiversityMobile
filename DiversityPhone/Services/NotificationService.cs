using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Disposables;

using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DiversityPhone.Interface;
using DiversityPhone.ViewModels;

namespace DiversityPhone.Services
{



    /// <summary>
    /// Provides global Notifications and Progress reporting via the Notification Tray
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly TimeSpan UPDATE_INTERVAL = TimeSpan.FromSeconds(3);

        ProgressIndicator _Progress = new ProgressIndicator() { IsVisible = true };

        int _ProgressCount = 0;
        ISubject<int> _ProgressCountSubject = new Subject<int>();
        IScheduler _Dispatcher;
        Stack<IObservable<string>> _Notifications = new Stack<IObservable<string>>();
        IObservable<string> _CurrentNotification;
        IDisposable _CurrentNotificationSubscription = Disposable.Empty;

        public NotificationService(
            PhoneApplicationFrame RootFrame,
            [Dispatcher] IScheduler Dispatcher
            )
        {
            RootFrame.Navigated += OnFrameNavigated;
            _Dispatcher = Dispatcher;

            _ProgressCountSubject
                .Select(c => c > 0)
                .Subscribe(setProgressindicator);
        }

        void OnFrameNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var page = e.Content as PhoneApplicationPage;
            if (page != null)
            {
                page.SetValue(SystemTray.ProgressIndicatorProperty, _Progress);
                page.SetValue(SystemTray.IsVisibleProperty, true);
            }
        }

        private void setProgressindicator(bool show)
        {
            _Dispatcher.Schedule(() =>
                {
                    if (show)
                    {
                        _Progress.IsIndeterminate = true;
                    }
                    else
                    {
                        _Progress.IsIndeterminate = false;
                    }
                    if (!_Progress.IsVisible)
                        _Progress.IsVisible = true;
                });
        }

        private void setNotification(string text)
        {
            _Dispatcher.Schedule(() => _Progress.Text = text);
        }

        private void updateNotification()
        {
            _Dispatcher.Schedule(() =>
                {
                    lock (this)
                    {
                        // The current Notification has ended and called us -> remove
                        if (_Notifications.Any() && _Notifications.Peek() == _CurrentNotification)
                            _Notifications.Pop();

                        // Check for active Notifications
                        if (_Notifications.Count > 0)
                        {
                            

                            _CurrentNotificationSubscription.Dispose();
                            _CurrentNotification = _Notifications.Peek();
                            _CurrentNotificationSubscription =
                                _CurrentNotification
                                    .DistinctUntilChanged()
                                    .Subscribe(n => setNotification(n), () => updateNotification());
                        }
                        else
                        {
                            _CurrentNotification = null;
                            _CurrentNotificationSubscription.Dispose();
                            setNotification("");
                        }
                    }
                });
        }

        private void addNotification(IObservable<string> notification)
        {
            var replays = notification.Replay(1);
            replays.Connect();
            lock (this)
            {
                _Notifications.Push(replays);
            }
            updateNotification();
        }

        private void adjustProgressCounter(bool up)
        {
            if (up)
                Interlocked.Increment(ref _ProgressCount);
            else
                Interlocked.Decrement(ref _ProgressCount);

            _ProgressCountSubject.OnNext(_ProgressCount);
        }

        public IDisposable showProgress(string text)
        {
            var observable = new BehaviorSubject<string>(text);
            showProgress(observable);
            return Disposable.Create(observable.OnCompleted);
        }

        public void showNotification(string text)
        {
            showNotification(text, UPDATE_INTERVAL);
        }

        public void showNotification(string text, TimeSpan duration)
        {
            var obs = Observable.Delay(Observable.Empty<string>(), duration)
                .StartWith(text);

            addNotification(obs);
        }

        public void showProgress(IObservable<string> text)
        {
            adjustProgressCounter(true);

            text.Subscribe(_ => { }, () => adjustProgressCounter(false));

            addNotification(text);
        }
    }
}
