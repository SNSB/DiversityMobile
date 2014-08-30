using ReactiveUI.Xaml;

namespace DiversityPhone.ViewModels
{
    public interface IDeletePageVM
    {
        IReactiveCommand Delete { get; }
    }
}