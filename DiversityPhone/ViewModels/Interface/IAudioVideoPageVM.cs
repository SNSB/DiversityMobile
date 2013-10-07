using ReactiveUI.Xaml;

namespace DiversityPhone.ViewModels
{
    public interface IAudioVideoPageVM : IEditPageVM
    {
        IReactiveCommand Play { get; }
        IReactiveCommand Stop { get; }
        PlayStates State { get; }
    }
}
