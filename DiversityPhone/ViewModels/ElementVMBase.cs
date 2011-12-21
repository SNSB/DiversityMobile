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
using ReactiveUI.Xaml;

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
        /// Operation: Select (Go To ViewXX Page)
        /// </summary>
        public ReactiveCommand Select { get; protected set; }
        /// <summary>
        /// Operation: Edit (Go To EditXX Page)
        /// </summary>
        public ReactiveCommand Edit { get; protected set; }

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


        public ElementVMBase(IMessageBus _messenger, T model)
        {
            this.Model = model;
            this.Messenger = _messenger;    
           
        }
    }
}
