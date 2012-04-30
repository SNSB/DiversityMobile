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
using DiversityPhone.DiversityService;
using System.Reactive.Linq;
using Funq;

namespace DiversityPhone.Services.BackgroundTasks
{
    public class DownloadTaxonListTask : BackgroundTask
    {       

        public override bool CanResume
        {
            get { return false; }
        }
        
        private ITaxonService Taxa;
        private IDiversityServiceClient Repo;

        public DownloadTaxonListTask(Container ioc) 
        {
            Taxa = ioc.Resolve<ITaxonService>();
            Repo = ioc.Resolve<IDiversityServiceClient>();
        }
        
        protected override void Run(object arg)
        {
            throw new NotImplementedException();
        }

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        public override void Cleanup(BackgroundTaskInvocation inv)
        {
            throw new NotImplementedException();
        }
    }
}
