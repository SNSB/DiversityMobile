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
using DiversityPhone.Services;
using ReactiveUI;
using System.Reactive.Linq;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{
    public abstract class ElementPageViewModel<T> : PageViewModel
    {
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
        /// Uses the default setting of doing Refreshing on the model.
        /// </summary>
        /// <param name="messenger"></param>
        public ElementPageViewModel(IMessageBus messenger)
            : this (messenger, true)
        {

        }

        /// <param name="messenger">Messenger</param>
        /// <param name="refreshModel">Determines whether or not the Model is updated, when the Page is refreshed</param>
        public ElementPageViewModel(IMessageBus messenger, bool refreshModel)
            : base(messenger)
        {
            var state = (refreshModel) ? StateObservable : DistinctStateObservable;
            var model = state
               .Select(s => ModelFromState(s))
               .Publish();
            ValidModel = model.Where(m => m != null);

            //Automatically navigate back, if there's no valid model.
            Messenger.RegisterMessageSource(
                model
                .Where(m => m == null)
                .Select(_ => Message.NavigateBack)
                );

            model.Connect();
        }
    }
}
