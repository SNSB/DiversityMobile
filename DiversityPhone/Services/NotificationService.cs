using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Disposables;
using Funq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DiversityPhone.Services
{   

    interface INotificationService
    {
        void showNotification(string text, TimeSpan timeout);
        void showProgress(IObservable<string> text);
    }    

    /// <summary>
    /// Provides global Notifications and Progress reporting via the Notification Tray
    /// </summary>
    class NotificationService : INotificationService
    {
        private readonly TimeSpan UPDATE_INTERVAL = TimeSpan.FromSeconds(3);

        ProgressIndicator _Progress = new ProgressIndicator() { IsIndeterminate = true, IsVisible = true };

        int _ProgressCount = 0;
        ISubject<int> _ProgressCountSubject = new Subject<int>();
        IScheduler _Dispatcher;
        List<IObservable<string>> _Notifications = new List<IObservable<string>>();
        int _CurrentNotificationIdx = -1;
        SerialDisposable _CurrentNotificationSubscription = new SerialDisposable();        

        public NotificationService(PhoneApplicationFrame rootFrame)
        {
            rootFrame.Navigated += OnFrameNavigated;


            _Dispatcher = DispatcherScheduler.Current;

            _ProgressCountSubject
                .Select(c => c > 0)
                .ObserveOnDispatcher()
                .Subscribe(setProgressindicator);
        }

        void OnFrameNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var page = e.Content as PhoneApplicationPage;
            if (page != null)
                page.SetValue(SystemTray.ProgressIndicatorProperty, _Progress);
        }

        private void setProgressindicator(bool show)
        {
            _Progress.Value = (show) ? 1.0 : 0.0;
        }

        private void setNotification(string text)
        {
            _Progress.Text = text;
        }

        private void updateNotification()
        {
            lock (this)
            {
                if (_Notifications.Count > 0)
                {
                    _CurrentNotificationIdx = ++_CurrentNotificationIdx % _Notifications.Count;
                    _CurrentNotificationSubscription.Disposable =
                        new CompositeDisposable(
                            new[] { 
                                _Notifications[_CurrentNotificationIdx].Subscribe(n => setNotification(n), () => removeNotification(_CurrentNotificationIdx)),
                                _Dispatcher.Schedule(UPDATE_INTERVAL, updateNotification)
                            });
                }
            }
        }

        private void addNotification(IObservable<string> notification)
        {
            var replays = notification.Replay(1);                
            replays.Connect();
            lock (this)
            {
                _Notifications.Add(replays.ObserveOnDispatcher());                  
            }
            updateNotification();
        }

        private void removeNotification(int idx)
        {
            lock (this)
            {
                _Notifications.RemoveAt(idx);
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

        public void showNotification(string text, TimeSpan timeout)
        {
            var obs = Observable.Return(text);
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
