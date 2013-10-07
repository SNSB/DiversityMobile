using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public interface IElementVM
    {
        string Description { get; }
        Icon Icon { get; }
        object Model { get; }
    }

    public interface IElementVM<T> : IElementVM, IReactiveNotifyPropertyChanged
    {
        new T Model { get; }
    }

    public static class ElementVMMixin
    {
        public static IObservable<T> Model<T>(this IObservable<IElementVM<T>> This)
        {
            return This.Select(vm => vm.Model);
        }        
    }
}
