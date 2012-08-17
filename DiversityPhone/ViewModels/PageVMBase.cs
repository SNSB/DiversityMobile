using ReactiveUI;
using System;
using System.Reactive.Subjects;
using System.Reactive.Linq;


namespace DiversityPhone.ViewModels
{
    public abstract class PageVMBase : ReactiveObject
    {
        public IMessageBus Messenger { get; private set; }


        private ISubject<bool> ActivationSubject = new Subject<bool>();
        public IObservable<bool> ActivationObservable { get; private set; }

        public void Activate() 
        {
            ActivationSubject.OnNext(true);
        }
        public void Deactivate() 
        {
            ActivationSubject.OnNext(false);
        }

        public PageVMBase()
        {
            Messenger = MessageBus.Current;
            ActivationObservable = ActivationSubject;
        }
    }

    public static class VMExtensions
    {
        public static IDisposable OnFirstActivation(this PageVMBase page, Action action)
        {
            if (page == null)
                throw new ArgumentNullException("page");
            if (action == null)
                throw new ArgumentNullException("action");

            return page.ActivationObservable.Where(active => active)
                .Take(1)
                .Subscribe(_ => action());
        }
    }
}
