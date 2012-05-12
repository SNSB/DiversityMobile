using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Services
{
    public interface IBackgroundService
    {
        /// <summary>
        /// Gets the registered Instance of the requested Task type
        /// </summary>
        /// <typeparam name="T">Task Type</typeparam>
        /// <returns>Registered Instance of T</returns>
        T getTaskObject<T>() where T : BackgroundTask;

        /// <summary>
        /// Starts an invocation of the given Background Task or queues it, if the Task is busy
        /// </summary>
        /// <typeparam name="T">Task Type</typeparam>
        /// <param name="argument">Argument for the Task ( Task specific )</param>
        void startTask<T>(object argument) where T : BackgroundTask;
    }
}
