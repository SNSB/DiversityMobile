using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI;
using ReactiveUI.Xaml;

namespace DiversityPhone.ViewModels
{
    public interface IEditPageVM : IReactiveNotifyPropertyChanged
    {
        IReactiveCommand Save { get; }
        IReactiveCommand ToggleEditable{ get; }
        IReactiveCommand Delete { get; }

        bool IsEditable { get; }
    }
}
