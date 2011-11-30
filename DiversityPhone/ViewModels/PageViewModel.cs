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
using DiversityPhone.Services;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace DiversityPhone.ViewModels
{
    public class PageViewModel : ReactiveObject
    {
        protected Subject<PageState> StateObservable { get; private set; }
        private ObservableAsPropertyHelper<PageState> _CurrentState;

        protected PageState CurrentState { get { return _CurrentState.Value; } }

        public  void SetState(PageState state)
        {
            StateObservable.OnNext(state);
        }    
        public virtual void SaveState(){}

        public PageViewModel()
        {
            StateObservable = new Subject<PageState>();
            _CurrentState = StateObservable.ToProperty(this, vm => vm.CurrentState);

        }
    }
}
