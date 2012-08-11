using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ReactiveUI.Xaml;

namespace DiversityPhone.ViewModels
{
    public interface IAudioVideoPageVM : IEditPageVM
    {
        IReactiveCommand Play { get; }
        IReactiveCommand Stop { get; }
    }
}
