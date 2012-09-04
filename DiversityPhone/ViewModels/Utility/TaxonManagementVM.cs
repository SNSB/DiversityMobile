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
using ReactiveUI;
using System.Reactive.Linq;
using System.Linq;
using DiversityPhone.Services;
using System.Collections.Generic;
using DiversityPhone.Model;
using Svc = DiversityPhone.DiversityService;
using ReactiveUI.Xaml;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using DiversityPhone.Services.BackgroundTasks;
using Funq;
using System.Reactive.Concurrency;
using System.Reactive;

namespace DiversityPhone.ViewModels
{
    public partial class TaxonManagementVM : PageVMBase
    {
        public enum Pivot
        {
            Local,
            Personal, 
            Public
        }

        private IConnectivityService Connectivity;
        private ITaxonService Taxa;
        private IDiversityServiceClient Service;
        private IBackgroundService Background;
        

        #region Properties

        private Pivot _CurrentPivot = Pivot.Local;
        public Pivot CurrentPivot 
        {
            get
            {
                return _CurrentPivot;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.CurrentPivot, ref _CurrentPivot, value);
            }
        }

        public bool IsBusy { get { return _IsBusy.Value; } }
        private ObservableAsPropertyHelper<bool> _IsBusy;       
        
        public ReactiveCollection<TaxonListVM> LocalLists { get; private set; }

        private ObservableAsPropertyHelper<IList<TaxonListVM>> _PersonalLists;
        public IList<TaxonListVM> PersonalLists { get { return _PersonalLists.Value; } }

        private ObservableAsPropertyHelper<IList<TaxonListVM>> _PublicLists;
        public IList<TaxonListVM> PublicLists { get { return _PublicLists.Value; } }

        public ReactiveCommand<TaxonListVM> Select { get; private set; }
        public ReactiveCommand<TaxonListVM> Download { get; private set; }
        public ReactiveCommand<TaxonListVM> Delete { get; private set; }
        public ReactiveCommand<TaxonListVM> Refresh { get; private set; }
        

        #endregion

        public TaxonManagementVM(Container ioc)
        {            
            Taxa = ioc.Resolve<ITaxonService>();
            Service = ioc.Resolve<IDiversityServiceClient>();
            Background = ioc.Resolve<IBackgroundService>();
            Connectivity = ioc.Resolve<IConnectivityService>();

            Select = new ReactiveCommand<TaxonListVM>(vm => !vm.IsSelected);
            Select.Subscribe(taxonlist =>
                    {
                        Taxa.selectTaxonList(taxonlist.Model);
                        taxonlist.IsSelected = true;
                    });

            Download = new ReactiveCommand<TaxonListVM>(vm => !vm.IsDownloading);
            Download
                .Subscribe(taxonlist =>
                        {
                            if (Taxa.getTaxonTableFreeCount() > 0)
                            {
                                var task = Background.getTaskObject<DownloadTaxonListTask>();
                                task.AsyncCompletedNotification
                                    .Where(arg => arg == taxonlist.Model)
                                    .Take(1)
                                    .ObserveOnDispatcher()
                                    .Subscribe(_ => taxonlist.IsDownloading = false);

                                task.AsyncErrorNotification
                                    .Where(arg => arg == taxonlist.Model)
                                    .Take(1)
                                    .ObserveOnDispatcher()
                                    .Subscribe(_ => { Taxa.deleteTaxonList(taxonlist.Model); LocalLists.Remove(taxonlist); });
                                     
                                CurrentPivot = Pivot.Local;
                                taxonlist.IsDownloading = true;
                                LocalLists.Add(taxonlist);
                                Taxa.addTaxonList(taxonlist.Model);
                                Background.startTask<DownloadTaxonListTask>(taxonlist.Model);


                            }
                            else
                                Messenger.SendMessage(new DialogMessage(Messages.DialogType.OK, DiversityResources.TaxonManagement_Message_Error,DiversityResources.TaxonManagement_Message_CantDownload));
                        });

            Delete = new ReactiveCommand<TaxonListVM>(vm => !vm.IsDownloading);
            Delete               
                .Subscribe(taxonlist =>
                    {
                        Taxa.deleteTaxonList(taxonlist.Model);
                        LocalLists.Remove(taxonlist);
                    });

            Refresh = new ReactiveCommand<TaxonListVM>(vm => !vm.IsDownloading);
            Refresh
                .Subscribe(taxonlist =>
                {
                    if (Delete.CanExecute(taxonlist)) //Deletes synchronously
                        Delete.Execute(taxonlist);

                    if (Download.CanExecute(taxonlist))
                        Download.Execute(taxonlist);
                });

            var local_lists =
            this.FirstActivation()
                .SelectMany(_ => Taxa.getTaxonSelections().ToObservable(Scheduler.ThreadPool)
                                .Select(list => new TaxonListVM(list)))
                .Publish();

            LocalLists = local_lists
                .ObserveOnDispatcher()
                .CreateCollection();

            local_lists.Connect();


            var local_lists_with_wifi = 
            local_lists.ToList()
                .CombineLatest(Connectivity.WifiAvailable().Where(wifi => wifi), (lists, _2) => lists)
                .Take(1);

            var online_lists =
            local_lists_with_wifi.SelectMany(local => 
                    Service.GetTaxonLists()
                    .Select(online => online.Select(l => local.Where(vm => vm.Model == l).FirstOrDefault() ?? new TaxonListVM(l)).ToList())
                )
                .Retry()    
                .ObserveOnDispatcher()                
                .Publish();

            _PersonalLists = this.ObservableToProperty(online_lists.Select(lists => lists.Where(l => !l.Model.IsPublicList).ToList() as IList<TaxonListVM>), x => x.PersonalLists);
            _PublicLists = this.ObservableToProperty(online_lists.Select(lists => lists.Where(l => l.Model.IsPublicList).ToList() as IList<TaxonListVM>), x => x.PublicLists);

            online_lists.Connect();
        }
    }
}
