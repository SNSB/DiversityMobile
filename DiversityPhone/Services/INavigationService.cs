namespace DiversityPhone.Services
{
    public interface INavigationService
    {
        void Navigate(Page p, string context);

        bool CanNavigateBack();

        void NavigateBack();

        void ClearHistory();
    }
}
