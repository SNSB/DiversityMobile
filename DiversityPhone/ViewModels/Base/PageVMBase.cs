using ReactiveUI;
using System;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive;


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
        public static IObservable<Unit> FirstActivation(this PageVMBase page)
        {
            return page.OnActivation()
                .Take(1);
        }

        public static IObservable<Unit> OnActivation(this PageVMBase page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            return page.ActivationObservable.Where(active => active)
                .Select(_ => Unit.Default);                
        }

        public static IObservable<Unit> OnDeactivation(this PageVMBase page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            return page.ActivationObservable.Where(active => !active)
                .Select(_ => Unit.Default);
        }
    }
}
