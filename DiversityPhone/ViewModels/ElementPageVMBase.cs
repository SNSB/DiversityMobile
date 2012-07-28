using DiversityPhone.Services;
using ReactiveUI;
using System.Reactive.Linq;
using DiversityPhone.Messages;
using ReactiveUI.Xaml;
using System;

namespace DiversityPhone.ViewModels
{
    public abstract class ElementPageVMBase<T> : PageViewModel
    {
        private IElementVM<T> _Current;
        /// <summary>
        /// Provides Access to the most recent Model Object
        /// </summary>
        public IElementVM<T> Current
        {
            get { return _Current; }
            protected set { this.RaiseAndSetIfChanged(x => x.Current, ref _Current, value); }
        }

        /// <summary>
        /// Provides the viewmodel retrieved from the PageState 
        /// if it is valid (i.e. non-null)
        /// </summary>
        protected IObservable<IElementVM<T>> CurrentObservable { get; private set; }

        public ElementPageVMBase ()
	    {
            DistinctStateObservable
                .Select(s => s.VMContext as IElementVM<T>)
                .Do(ctx =>
                {
                    if(ctx == null)
                        Messenger.SendMessage(Page.Previous);
                })
                .Where(ctx => ctx != null)
                .BindTo(this, x => x.Current);
	    
            var currentObs =
            this.ObservableForProperty(x => x.Current)
                .Value()
                .Publish();                
            CurrentObservable = currentObs;
            currentObs.Connect();
        }
    }
}
