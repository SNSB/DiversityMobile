namespace DiversityPhone.ViewModels {
    using DiversityPhone.Interface;
    using ReactiveUI;
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;


    public abstract class PageVMBase : ReactiveObject {
        private static IMessageBus _Messenger;
        private static INotificationService _Notifications;

        public static void Initialize(IMessageBus M, INotificationService N) {
            _Messenger = M;
            _Notifications = N;
        }

        public IMessageBus Messenger { get { return _Messenger; } }
        public INotificationService Notifications { get { return _Notifications; } }

        private ISubject<bool> ActivationSubject = new Subject<bool>();
        public IObservable<bool> ActivationObservable { get; private set; }

        public void Activate() {
            ActivationSubject.OnNext(true);
        }
        public void Deactivate() {
            ActivationSubject.OnNext(false);
        }

        public PageVMBase() {
            ActivationObservable = ActivationSubject;
        }
    }

    public static class VMExtensions {
        public static IObservable<Unit> FirstActivation(this PageVMBase page) {
            return page.OnActivation()
                .Take(1);
        }

        public static IObservable<Unit> OnActivation(this PageVMBase page) {
            if (page == null)
                throw new ArgumentNullException("page");

            return page.ActivationObservable.Where(active => active)
                .Select(_ => Unit.Default);
        }

        public static IObservable<Unit> OnDeactivation(this PageVMBase page) {
            if (page == null)
                throw new ArgumentNullException("page");

            return page.ActivationObservable.Where(active => !active)
                .Select(_ => Unit.Default);
        }
    }
}
