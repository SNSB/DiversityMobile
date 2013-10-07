using ReactiveUI;
using ReactiveUI.Xaml;

namespace DiversityPhone.ViewModels
{
    public interface ISavePageVM : IReactiveNotifyPropertyChanged
    {
        IReactiveCommand Save { get; }
        IReactiveCommand ToggleEditable { get; }
        bool IsEditable { get; }
    }
}
