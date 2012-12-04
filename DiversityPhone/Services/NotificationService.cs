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
        IDisposable showProgress(string text);
        void showNotification(string text, TimeSpan duration);
        void showProgress(IObservable<string> text);       
    }    

    /// <summary>
    /// Provides global Notifications and Progress reporting via the Notification Tray
    /// </summary>
    class NotificationService : INotificationService
    {
        private readonly TimeSpan UPDATE_INTERVAL = TimeSpan.FromSeconds(3);

        ProgressIndicator _Progress = new ProgressIndicator() { IsVisible = true };

        int _ProgressCount = 0;
        ISubject<int> _ProgressCountSubject = new Subject<int>();
        IScheduler _Dispatcher;
        List<IObservable<string>> _Notifications = new List<IObservable<string>>();
        IObservable<string> _CurrentNotification;
        IDisposable _CurrentNotificationSubscription = Disposable.Empty;        

        public NotificationService(Container ioc)
        {
            var rootFrame = ioc.Resolve<PhoneApplicationFrame>();
            rootFrame.Navigated += OnFrameNavigated;


            _Dispatcher = ioc.ResolveNamed<IScheduler>(NamedServices.DISPATCHER);

            _ProgressCountSubject
                .Select(c => c > 0)                
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
            lock (this)
            {
                if (_Notifications.Count > 0)
                {                    
                    var nextNotification = _Notifications[(_Notifications.IndexOf(_CurrentNotification) + 1) % _Notifications.Count];
                    if (nextNotification != _CurrentNotification)
                    {
                        _CurrentNotification = nextNotification;
                        _CurrentNotificationSubscription.Dispose();
                        _CurrentNotificationSubscription =
                            new CompositeDisposable(
                                new[] { 
                                    _CurrentNotification
                                        .DistinctUntilChanged()
                                        .Subscribe(n => setNotification(n), () => removeNotification(_CurrentNotification)),
                                    _Dispatcher.Schedule(UPDATE_INTERVAL, updateNotification)
                                });
                    }
                }
                else
                {
                    _CurrentNotification = null;
                    _CurrentNotificationSubscription.Dispose();
                    setNotification("");
                }
            }
        }

        private void addNotification(IObservable<string> notification)
        {        

            var replays = notification.Replay(1);                
            replays.Connect();
            lock (this)
            {
                _Notifications.Add(replays);                  
            }
            _Dispatcher.Schedule(updateNotification);
        }

        private void removeNotification(IObservable<string> not)
        {
            lock (this)
            {
                _Notifications.Remove(not);
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
