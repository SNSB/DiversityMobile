using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Services
{
    public interface IBackgroundProcess
    {
        string ProcessName { get; }
        void Run();
        void Cancel();

    }
}
