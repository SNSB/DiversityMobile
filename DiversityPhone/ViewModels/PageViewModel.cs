using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using System.Reactive.Linq;
using DiversityPhone.Services;
using System.Collections.Generic;
using System.Reactive.Subjects;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{
    public abstract class PageViewModel : ReactiveObject
    {
        protected IMessageBus Messenger { get; private set; }
        private Subject<PageState> _StateObservable;
        /// <summary>
        /// Observable Sequence of incoming PageStates.
        /// Guaranteed to be non-null
        /// Not Distinct (Can be used for Refreshing)
        /// </summary>
        protected IObservable<PageState> StateObservable 
        { 
            get 
            { 
                return _StateObservable; 
            } 
        }
        /// <summary>
        /// Observable Sequence of incoming PageStates.
        /// Guaranteed to be non-null
        /// Distinct (Does no Refreshing)
        /// </summary>
        protected IObservable<PageState> DistinctStateObservable
        {
            get
            {
                return _StateObservable.DistinctUntilChanged();
            }
        }

        private ObservableAsPropertyHelper<PageState> _CurrentState;
        /// <summary>
        /// Always returns the most recent State        
        /// </summary>
        protected PageState CurrentState { get { return _CurrentState.Value; } }

        


        public void SetState(PageState state)
        {
            if(state != null)
                _StateObservable.OnNext(state);
        }    
        public virtual void SaveState(){}

        public PageViewModel(IMessageBus messenger)
        {
            Messenger = messenger;
            _StateObservable = new Subject<PageState>();

			_CurrentState = StateObservable
				.ToProperty(this, vm => vm.CurrentState);
           
        }
    }
}
