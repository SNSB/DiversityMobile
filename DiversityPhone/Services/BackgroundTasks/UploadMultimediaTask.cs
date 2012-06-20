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
    public class UploadMultimediaTask : BackgroundTask
    {
        private const string ARGUMENT_KEY = "A";
        private IFieldDataService Storage;
        private IDiversityServiceClient Repository;

        public UploadMultimediaTask(Container ioc)
        {
            Storage = ioc.Resolve<IFieldDataService>();
            Repository = ioc.Resolve<IDiversityServiceClient>();
        }


        public override bool CanResume
        {
            get { return false; }
        }

        protected override void saveArgumentToState(object arg)
        {
            var mmo = arg as MultimediaObject;
            if (mmo != null)
            {
                State[ARGUMENT_KEY] = mmo.MMOID.ToString();
            }
        }

        protected override object getArgumentFromState()
        {
            if (State.ContainsKey(ARGUMENT_KEY))
            {
                return Storage.getMultimediaByID(int.Parse(State[ARGUMENT_KEY]));
            }
            return null;
        }

        protected override void Run(object arg)
        {
            var mmo = arg as MultimediaObject;
            if (mmo != null)
            {
                //TODO
            }
        }

        protected override void Cancel()
        {
            //Nothing to do?
        }

        protected override void Cleanup(object arg)
        {
            //Nothing to do?
        }
    }
}
