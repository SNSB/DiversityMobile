using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI.Xaml;

namespace DiversityPhone.ViewModels
{
    public interface IDeletePageVM
    {
        IReactiveCommand Delete { get; }
    }
}
