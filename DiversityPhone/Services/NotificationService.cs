namespace DiversityPhone.Services {
    using DiversityPhone.Interface;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Windows;


    /// <summary>
    /// Provides global Notifications and Progress reporting via the Notification Tray
    /// </summary>
    public class NotificationService : INotificationService {
        private static readonly ProgressState IDLE_STATE = new ProgressState(0, string.Empty);

        ProgressIndicator _Progress = new ProgressIndicator() { IsVisible = true };

        public IScheduler NotificationScheduler { get; private set; }
        List<IObservable<ProgressState>> _Notifications = new List<IObservable<ProgressState>>();
        IDisposable _CurrentNotificationSubscription = Disposable.Empty;

        public NotificationService(
            PhoneApplicationFrame RootFrame,
            [Dispatcher] IScheduler Dispatcher
            ) {
            RootFrame.Navigated += OnFrameNavigated;
            NotificationScheduler = Dispatcher;
        }

        void OnFrameNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e) {
            var page = e.Content as PhoneApplicationPage;
            if (page != null) {
                page.SetValue(SystemTray.ProgressIndicatorProperty, _Progress);
                page.SetValue(SystemTray.IsVisibleProperty, true);
            }
        }

        private void setNotification(ProgressState state) {
            NotificationScheduler.Schedule(() => {
                _Progress.Text = state.ProgressMessage;
                _Progress.Value = state.ProgressPercentage ?? 0;
                _Progress.IsIndeterminate = !state.ProgressPercentage.HasValue;
                _Progress.IsVisible = true;
            });
        }

        private void removeNotification(IObservable<ProgressState> noti) {
            NotificationScheduler.Schedule(() => {
                lock (this) {
                    _Notifications.Remove(noti);
                }
                updateNotification();
            });
        }

        private void updateNotification() {
            NotificationScheduler.Schedule(() => {
                lock (this) {
                    _CurrentNotificationSubscription.Dispose();
                    // Check for active Notifications
                    if (_Notifications.Count > 0) {
                        var current = _Notifications[0];
                        _CurrentNotificationSubscription =
                            current
                                .Subscribe(setNotification);
                    }
                    else {
                        setNotification(IDLE_STATE);
                    }
                }
            });
        }

        public void showProgress(IObservable<ProgressState> progress) {
            var replays = progress.Replay(1);
            progress.Finally(() => removeNotification(replays)).Subscribe();
            lock (this) {
                _Notifications.Insert(0, replays);
            }
            replays.Connect();
            updateNotification();
        }

        public IObservable<Unit> showPopup(string text) {
            return Observable.Start(() => { MessageBox.Show(text); }, NotificationScheduler);
        }

        public IObservable<bool> showDecision(string text) {
            return Observable.Start(() => MessageBox.Show(text, string.Empty, MessageBoxButton.OKCancel), NotificationScheduler)
                .Select(res => (res == MessageBoxResult.OK || res == MessageBoxResult.Yes) ? true : false);
        }
    }
}
