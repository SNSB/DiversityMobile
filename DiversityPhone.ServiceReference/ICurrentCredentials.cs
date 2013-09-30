namespace DiversityPhone.Services
{
    using DiversityPhone.Model;
    using System;

    public interface ICredentialsService
    {
        IObservable<UserCredentials> CurrentCredentials();
    }
}
