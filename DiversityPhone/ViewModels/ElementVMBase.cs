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
    public abstract class ElementVMBase<T> : ReactiveObject
    {
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

        public ISubject<bool> CanSelect { get; private set; }

        public IObservable<IElementVM<T>> SelectObservable { get; private set; }

        protected ISubject<IElementVM<T>> SelectSubject = new Subject<IElementVM<T>>();

        public ElementVMBase(T model)
        {
            this.Model = model;
            this.CanSelect = new Subject<bool>();              
            this.Select = new ReactiveCommand(CanSelect); 
       
            var selectPublish =
                SelectSubject
                    .Publish();
            selectPublish.Connect();
            SelectObservable = selectPublish;
                
            Select
                .Select(_ => this as IElementVM<T>)
                .Subscribe(SelectSubject);
        }        
    }   
}
