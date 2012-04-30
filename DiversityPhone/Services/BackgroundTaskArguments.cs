using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Services
{
    public abstract class BackgroundTaskArguments : IComparable<BackgroundTaskArguments>
    {
        string TaskName { get; private set; }
        bool CanResume { get; private set; }


        public BackgroundTaskArguments(string name, bool canresume)
        {
            TaskName = name;
            CanResume = canresume;
        }

        public int CompareTo(BackgroundTaskArguments other)
        {
            return TaskName.CompareTo(other.TaskName);
        }
    }
}
