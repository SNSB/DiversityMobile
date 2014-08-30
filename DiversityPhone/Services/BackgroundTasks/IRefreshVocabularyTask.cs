namespace DiversityPhone.Interface
{
    using DiversityPhone.Model;
    using System;
    using System.Reactive;

    public interface IRefreshVocabularyTask
    {
        IObservable<Unit> Start(UserCredentials login);
    }
}