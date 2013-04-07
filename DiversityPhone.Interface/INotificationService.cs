using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Interface
{
    public interface INotificationService
    {
        IDisposable showProgress(string text);
        void showNotification(string text, TimeSpan duration);
        void showNotification(string text);
        void showProgress(IObservable<string> text);
    }  
}
