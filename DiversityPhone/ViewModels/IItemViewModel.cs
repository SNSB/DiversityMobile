using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace DiversityPhone.ViewModels
{
    public interface IItemViewModel 
    {
        Icon Icon { get; }
        string Title { get; }
        string Description { get; }

        ICommand Select { get; }
    }
}
