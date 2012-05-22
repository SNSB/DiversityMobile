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
using System.ServiceModel;

namespace DiversityPhone.Services.BackgroundTasks
{
    public class DownloadTaxonListTask : BackgroundTask
    {
        private const string KEY_TABLE = "T";
        private const string KEY_NAME = "N";
        private const string KEY_GROUP = "G";

        private const string KEY_PROGRESS = "S";
        private const string STATE_INITIAL = "I";
        private const string STATE_STARTED = "S";
        private const string STATE_FINISHED = "F";

        private string CurrentState
        {
            get
            {
                string res;
                if (State.TryGetValue(KEY_PROGRESS, out res))
                    return res;
                else
                    return STATE_INITIAL;
            }
            set
            {                
                State[KEY_PROGRESS] = value;
            }
        }


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
            var list = arg as TaxonList;            

            if (list != null)
            {

                while (CurrentState != STATE_FINISHED)
                {
                    if (CurrentState == STATE_STARTED)
                        Cleanup(list);

                    CurrentState = STATE_STARTED;

                    try
                    {  
                        //If the service is unavailable, the resulting exceptions abort the execution
                        Repo.DownloadTaxonListChunked(list)
                        .ForEach(chunk => Taxa.addTaxonNames(chunk, list));                                           

                        CurrentState = STATE_FINISHED;
                    }
                    catch (WebException ex) // On app resume, catch webexception (and retry)
                    {
                        var t = ex.Message;
                    }
                    
                }
            }
        }

        protected override void saveArgumentToState(object arg)
        {      
            var list = arg as TaxonList;
            if(list != null)
            {
                State[KEY_NAME] = list.DisplayText;
                State[KEY_TABLE] = list.Table;
                State[KEY_GROUP] = list.TaxonomicGroup;
            }
        }

        protected override object  getArgumentFromState()
        {
            return new TaxonList()
            {
                DisplayText = State[KEY_NAME],
                Table = State[KEY_TABLE],
                TaxonomicGroup = State[KEY_GROUP]
            };            
        }

        protected override void Cancel()
        {
            
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
