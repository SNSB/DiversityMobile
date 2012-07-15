using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using NLog;
using NLog.LogReceiverService;

namespace DiversityService.LogReciever
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Klassennamen "Service1" sowohl im Code als auch in der SVC- und der Konfigurationsdatei ändern.
    public class DiversityLogService : ILogReceiverServer
    {

        public void ProcessLogMessages(NLogEvents nevents)
        {
            var events = nevents.ToEventInfo("Client.");
            Console.WriteLine("in: {0} {1}", nevents.Events.Length, events.Count);

            foreach (var ev in events)
            {
                var logger = LogManager.GetLogger(ev.LoggerName);
                logger.Log(ev);
            }
        }
    }
}
