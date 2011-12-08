namespace DiversityPhone.Services
{
    public interface INavigationService
    {
        void Navigate(Page p, string context, string referrer);

        bool CanNavigateBack();

        void NavigateBack();

        void ClearHistory();
    }
}
