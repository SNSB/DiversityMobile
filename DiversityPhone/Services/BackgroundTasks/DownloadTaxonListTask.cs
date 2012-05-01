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
using DiversityPhone.ViewModels;
using System.Collections.Generic;

namespace DiversityPhone.Services.BackgroundTasks
{
    public class DownloadTaxonListTask : BackgroundTask
    {
        private const string TABLE = "T";
        private const string NAME = "N";
        private const string GROUP = "G";



        public override bool CanResume
        {
            get { return false; }
        }
        
        private ITaxonService Taxa;
        private IDiversityServiceClient Repo;
        private bool isCancelled;

        public DownloadTaxonListTask(Container ioc) 
        {
            Taxa = ioc.Resolve<ITaxonService>();
            Repo = ioc.Resolve<IDiversityServiceClient>();
        }
        
        protected override void Run(object arg)
        {
            var list = arg as TaxonList;            

            if (list != null)
            {                
                isCancelled = false;
                try
                {
                    Repo.DownloadTaxonListChunked(list)
                    .TakeWhile(_ => !isCancelled)
                    .ForEach(chunk => Taxa.addTaxonNames(chunk, list));
                }
                catch (Exception ex)
                {

                    
                }                
            }
        }

        protected override void saveArgumentToState(object arg)
        {      
            var list = arg as TaxonList;
            if(list != null)
            {
                State[NAME] = list.DisplayText;
                State[TABLE] = list.Table;
                State[GROUP] = list.TaxonomicGroup;
            }
        }

        protected override object  getArgumentFromState()
        {
            return new TaxonList()
            {
                DisplayText = State[NAME],
                Table = State[TABLE],
                TaxonomicGroup = State[GROUP]
            };            
        }

        public override void Cancel()
        {
            isCancelled = true;
        }

        protected override void Cleanup(object arg)
        {
            var list = arg as TaxonList;
            if (list != null)
            {                
                Taxa.deleteTaxonList(list);
            }
        }
    }
}
