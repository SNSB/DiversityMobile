using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace DiversityPhone.Services
{
    public class BackgroundTaskFactory : Dictionary<string, Func<BackgroundTaskArguments,BackgroundTask>>, IBackgroundTaskFactory
    {
        public BackgroundTask createTask(BackgroundTaskArguments args)
        {
            var ID = args.TaskName;
            if (this.ContainsKey(ID))
            {
                return this[ID](args);
            }
            else
                return null;
        }
    }
}
