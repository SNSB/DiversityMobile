using DiversityPhone.Interface;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace DiversityPhone.Services
{



    /// <summary>
    /// Provides global Notifications and Progress reporting via the Notification Tray
    /// </summary>
    public class NotificationService : INotificationService
    {
        private static readonly ProgressState IDLE_STATE = new ProgressState(0, string.Empty);

        ProgressIndicator _Progress = new ProgressIndicator() { IsVisible = true };

        int _ProgressCount = 0;
        ISubject<int> _ProgressCountSubject = new Subject<int>();
        public IScheduler NotificationScheduler { get; private set; }
        Stack<IObservable<ProgressState>> _Notifications = new Stack<IObservable<ProgressState>>();
        IObservable<ProgressState> _CurrentNotification;
        IDisposable _CurrentNotificationSubscription = Disposable.Empty;

        public NotificationService(
            PhoneApplicationFrame RootFrame,
            [Dispatcher] IScheduler Dispatcher
            )
        {
            RootFrame.Navigated += OnFrameNavigated;
            NotificationScheduler = Dispatcher;

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
            NotificationScheduler.Schedule(() =>
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

        private void setNotification(ProgressState state)
        {
            NotificationScheduler.Schedule(() =>
                {
                    _Progress.Text = state.ProgressMessage;
                    _Progress.Value = state.ProgressPercentage ?? 0;
                    _Progress.IsIndeterminate = !state.ProgressPercentage.HasValue;
                });
        }

        private void updateNotification()
        {
            NotificationScheduler.Schedule(() =>
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
                            setNotification(IDLE_STATE);
                        }
                    }
                });
        }

        private void addNotification(IObservable<ProgressState> notification)
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

        public void showProgress(IObservable<ProgressState> progress)
        {
            adjustProgressCounter(true);

            progress.Subscribe(_ => { }, () => adjustProgressCounter(false));

            addNotification(progress);
        }
    }
}
