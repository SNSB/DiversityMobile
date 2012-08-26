using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI.Xaml;
using ReactiveUI;

namespace DiversityPhone.ViewModels
{
    public interface ISavePageVM : IReactiveNotifyPropertyChanged
    {
        IReactiveCommand Save { get; }
        IReactiveCommand ToggleEditable { get; }
        bool IsEditable { get; }
    }
}
