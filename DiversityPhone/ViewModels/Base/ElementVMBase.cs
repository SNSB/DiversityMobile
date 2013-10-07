using ReactiveUI;

namespace DiversityPhone.ViewModels
{
    /// <summary>
    /// Base class for all Element ViewModels (VMs, that represent a single item of a Model Class).    /// 
    /// </summary>
    /// <typeparam name="T">The Model class that this VM will encapsulate.</typeparam>
    public abstract class ElementVMBase<T> : ReactiveObject, IElementVM<T>
    { 
        /// <summary>
        /// Encapsulated Model Instance
        /// </summary>
        public T Model { get; protected set; }

        /// <summary>
        /// string to Display for this Object
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Icon to display for this Object
        /// </summary>
        public abstract Icon Icon { get; }

        public ElementVMBase(T model)
        {
            this.Model = model;
        }

        object IElementVM.Model
        {
            get { return Model; }
        }
    }   
}
