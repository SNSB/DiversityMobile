using System;
using ReactiveUI;
using ReactiveUI.Xaml;
using System.Reactive.Linq;
using DiversityPhone.Services;
using DiversityPhone.Messages;
using System.Reactive.Subjects;

namespace DiversityPhone.ViewModels
{
    /// <summary>
    /// Base class for all Element ViewModels (VMs, that represent a single item of a Model Class).    /// 
    /// </summary>
    /// <typeparam name="T">The Model class that this VM will encapsulate.</typeparam>
    public abstract class ElementVMBase<T>
    {
        /// <summary>
        /// Preinitialized Messenger Instance 
        /// </summary>
        protected IMessageBus Messenger { get; private set; }

        /// <summary>
        /// Operation: Select (Go To Target Page)
        /// </summary>
        public ReactiveCommand Select { get; private set; }
        
        /// <summary>
        /// Encapsulated Model Instance
        /// </summary>
        public T Model { get; private set; }

        /// <summary>
        /// String to Display for this Object
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Icon to display for this Object
        /// </summary>
        public abstract Icon Icon { get; }

        /// <summary>
        /// Enables and Disables the Select Command
        /// </summary>
        protected Subject<bool> CanSelect { get; private set; }

        /// <summary>
        /// The Page, that the Select Command will navigate to.
        /// </summary>
        protected Page TargetPage { get; private set; }

        /// <summary>
        /// Retrieves the Context string to be sent on Select
        /// </summary>
        protected abstract NavigationMessage NavigationMessage { get; }


        public ElementVMBase(IMessageBus _messenger, T model, Page targetPage)
        {
            this.Model = model;
            this.Messenger = _messenger;
            this.CanSelect = new Subject<bool>();                    
            this.Select = new ReactiveCommand(CanSelect);

            CanSelect.OnNext(true);

            Messenger.RegisterMessageSource(
                Select
                .Select(_ => NavigationMessage)
                );
           
        }
    }
}
