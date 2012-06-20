using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI.Xaml;
using System.Reactive.Subjects;

namespace DiversityPhone.ViewModels
{
    public interface IElementVM<out T>
    {
        /// <summary>
        /// Operation: Select (Go To Target Page)
        /// </summary>
        public ReactiveCommand Select { get; }

        /// <summary>
        /// Encapsulated Model Instance
        /// </summary>
        public T Model { get; }

        /// <summary>
        /// String to Display for this Object
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Icon to display for this Object
        /// </summary>
        public abstract Icon Icon { get; }

        public ISubject<bool> CanSelect { get; }

        public IObservable<IElementVM<T>> SelectObservable { get;  }

    }
}
