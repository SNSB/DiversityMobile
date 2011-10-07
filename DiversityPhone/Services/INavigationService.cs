namespace DiversityPhone.Services
{
    public interface INavigationService
    {
        void Navigate(Page p);

        bool CanNavigateBack();

        void NavigateBack();

        void ClearHistory();
    }
}
