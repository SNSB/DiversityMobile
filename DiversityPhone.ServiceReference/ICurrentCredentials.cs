namespace DiversityPhone.Services
{
    using DiversityPhone.Model;

    public interface ICredentialsService
    {
        UserCredentials CurrentCredentials();
    }
}
