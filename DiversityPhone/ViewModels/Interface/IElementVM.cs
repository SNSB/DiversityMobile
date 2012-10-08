using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using ReactiveUI;

namespace DiversityPhone.ViewModels
{
    public interface IElementVM<T> : IReactiveNotifyPropertyChanged
    {
        T Model { get; }
    }

    public static class ElementVMMixin
    {
        public static IObservable<T> Model<T>(this IObservable<IElementVM<T>> This)
        {
            return This.Select(vm => vm.Model);
        }
    }
}
