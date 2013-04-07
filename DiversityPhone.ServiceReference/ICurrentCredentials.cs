namespace DiversityPhone.Services
{
    using DiversityPhone.Model;

    public interface ICurrentCredentials
    {
        UserCredentials CurrentCredentials();
    }
}
