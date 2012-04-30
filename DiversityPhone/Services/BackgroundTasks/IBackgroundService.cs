using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Services
{
    public interface IBackgroundService
    {
        T getTaskObject<T>() where T : BackgroundTask;
    }
}
