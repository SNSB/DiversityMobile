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
using DiversityPhone.Model;
using Funq;

namespace DiversityPhone.Services.BackgroundTasks
{
    public class UploadSeriesTask : BackgroundTask
    {
        private const string SERIES_KEY = "S";

        IDiversityServiceClient Repo;
        IFieldDataService Storage;

        public UploadSeriesTask(Container ioc)
        {
            Repo = ioc.Resolve<IDiversityServiceClient>();
            Storage = ioc.Resolve<IFieldDataService>();
        }


        public override bool CanResume
        {
            get { return true; }
        }

        protected override void saveArgumentToState(object arg)
        {
            var series = arg as EventSeries;
            if (series != null)
            {
                State[SERIES_KEY] = series.SeriesID.ToString();
            }
        }

        protected override object getArgumentFromState()
        {
            int id;
            if (State.ContainsKey(SERIES_KEY) && int.TryParse(State[SERIES_KEY], out id))
            {
                return Storage.getEventSeriesByID(id);
            }

            return null;
        }

        protected override void Run(object arg)
        {
            var series = arg as EventSeries;
            if (series != null)
            {
                if (series.IsModified())
                    ;
            }
        }

        protected override void Cancel()
        {
            throw new NotImplementedException();
        }

        protected override void Cleanup(object arg)
        {
            throw new NotImplementedException();
        }
    }
}
