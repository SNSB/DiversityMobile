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
        ReactiveCommand Select { get; }

        /// <summary>
        /// Encapsulated Model Instance
        /// </summary>
        T Model { get; }

        /// <summary>
        /// String to Display for this Object
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Icon to display for this Object
        /// </summary>
        Icon Icon { get; }

        ISubject<bool> CanSelect { get; }

        //IObservable<IElementVM<T>> SelectObservable { get;  }

    }
}
