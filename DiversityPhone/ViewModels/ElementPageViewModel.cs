using DiversityPhone.Services;
using ReactiveUI;
using System.Reactive.Linq;
using DiversityPhone.Messages;
using ReactiveUI.Xaml;
using System;

namespace DiversityPhone.ViewModels
{
    public abstract class ElementPageViewModel<T> : PageViewModel
    {
        protected IFieldDataService Storage { get; private set; }


        private ObservableAsPropertyHelper<ElementVMBase<T>> _Current;
        /// <summary>
        /// Provides Access to the most recent Model Object
        /// </summary>
        public ElementVMBase<T> Current
        {
            get { return _Current.Value; }
        }

        /// <summary>
        /// Provides the viewmodel retrieved from the PageState 
        /// if it is valid (i.e. non-null)
        /// </summary>
        protected IObservable<ElementVMBase<T>> CurrentObservable { get; private set; }

        /// <summary>
        /// Provides the model retrieved from the PageState 
        /// if it is valid (i.e. non-null)        
        /// </summary>
        protected IObservable<T> ValidModel { get; private set; }

        /// <summary>
        /// Provides a Model from a given Page State
        /// </summary>
        /// <param name="s">state</param>
        /// <returns>a Model object associated with this state, or null if there is none</returns>
        protected abstract T ModelFromState(PageState s);

        /// <summary>
        /// Provides a ViewModel for the Current Model
        /// </summary>
        /// <param name="model">Current Model</param>
        /// <returns>ViewModel for it</returns>
        protected abstract ElementVMBase<T> ViewModelFromModel(T model);
        /// <summary>
        /// Uses the default setting of doing Refreshing on the model.
        /// </summary>
        /// <param name="messenger"></param>
        public ElementPageViewModel()
            : this (true)
        {

        }

        public ElementPageViewModel(bool refreshModel)
            : this(MessageBus.Current, App.OfflineDB, refreshModel)
        {

        }

        /// <param name="messenger">Messenger</param>
        /// <param name="refreshModel">Determines whether or not the Model is updated, when the Page is refreshed</param>
        public ElementPageViewModel(IMessageBus messenger, IFieldDataService storage, bool refreshModel)
            : base(messenger)
        {
            Storage = storage;            

            var state = (refreshModel) ? StateObservable : DistinctStateObservable;
            var model = state
               .Select(s => ModelFromState(s))
               .Publish();
            model.Connect();
            var currentObs = model
                .Where(m => m != null)
                .Select(m => ViewModelFromModel(m))
                .Publish();                
            CurrentObservable = currentObs;
            currentObs.Connect();

            ValidModel = CurrentObservable
                .Select(vm => vm.Model);

            _Current = CurrentObservable                
                .ToProperty(this, x => x.Current);            

            //Automatically navigate back, if there's no valid model.
            Messenger.RegisterMessageSource(
                model
                .Where(m => m == null)
                .Select(_ => Page.Previous)
                );

            
        }
    }
}
