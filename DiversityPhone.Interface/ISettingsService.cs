using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Interface
{
    public interface ISettingsService
    {
        AppSettings getSettings();
        void saveSettings(AppSettings settings);
    }
}
