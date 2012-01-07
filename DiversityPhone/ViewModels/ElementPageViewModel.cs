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


        public ElementPageViewModel(IMessageBus messenger)
            : base(messenger)
        {
            var model = StateObservable
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
