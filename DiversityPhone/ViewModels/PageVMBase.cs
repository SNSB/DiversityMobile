using ReactiveUI;
using System;
using System.Reactive.Subjects;


namespace DiversityPhone.ViewModels
{
    public abstract class PageVMBase : ReactiveObject
    {
        public IMessageBus Messenger { get; private set; }


        private ISubject<bool> ActivationSubject = new Subject<bool>();
        protected IObservable<bool> ActivationObservable { get; private set; }

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
}
