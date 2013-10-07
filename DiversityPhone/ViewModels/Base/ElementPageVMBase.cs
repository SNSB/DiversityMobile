using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public abstract class ElementPageVMBase<T> : PageVMBase
    {
        private IElementVM<T> _Current;
        /// <summary>
        /// Provides Access to the most recent Model Object
        /// </summary>
        public IElementVM<T> Current
        {
            get { return _Current; }
            set { this.RaiseAndSetIfChanged(x => x.Current, ref _Current, value); } //Public for Binding
        }

        /// <summary>
        /// Provides the viewmodel retrieved from the PageState 
        /// if it is valid (i.e. non-null)
        /// </summary>
        protected IObservable<IElementVM<T>> CurrentObservable { get; private set; }

        /// <summary>
        /// Fires whenever the current model Instance changes.
        /// Doesn't fire, of the page was deactivated and activated again, with the same model instance
        /// </summary>
        protected IObservable<T> CurrentModelObservable { get; private set; }

        /// <summary>
        /// Fires on every Page Activation, providing the current model Instance even if it hasn't changed from last time
        /// </summary>
        protected IObservable<T> ModelByVisitObservable { get; private set; }

        public ElementPageVMBase ()
	    {    
            var currentObs =
            this.ObservableForProperty(x => x.Current)
                .Value()
                .Publish();                
            CurrentObservable = currentObs;
            currentObs.Connect();

            var modelObs =
                CurrentObservable
                .Select(vm => vm.ObservableForProperty(x => x.Model))
                .Switch()
                .Value()
                .Merge(CurrentObservable.Select(vm => vm.Model))                
                .Publish();
            CurrentModelObservable = modelObs;
            modelObs.Connect();

            var modelByVisit = this.OnActivation()
                .CombineLatest(CurrentModelObservable, (_, m) => m)
                .Publish();
            ModelByVisitObservable = modelByVisit;
            modelByVisit.Connect();            
        }
    }
}
