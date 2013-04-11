using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Interface
{
    public interface ISettingsService
    {
        IObservable<AppSettings> CurrentSettings();
        void SaveSettings(AppSettings settings);
    }
}
