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
using Client = DiversityPhone.Model;
using DiversityPhone.DiversityService;
using ReactiveUI.Xaml;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using DiversityPhone.Services.BackgroundTasks;
using Funq;

namespace DiversityPhone.ViewModels
{
    public partial class TaxonManagementVM : PageViewModel
    {
        public enum Pivot
        {
            Local,
            Repository
        }

        #region Services
        private ITaxonService Taxa { get; set; }        
        private IDiversityServiceClient Service { get; set; }
        private IBackgroundService Background { get; set; }
        #endregion

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

        private IList<Client.TaxonSelection> _TaxonSelections;
        public ObservableCollection<TaxonListVM> LocalLists { get; private set; }
        public ObservableCollection<TaxonListVM> RepoLists { get; private set; } 
     
        private ReactiveAsyncCommand getRepoLists = new ReactiveAsyncCommand();
        private IBackgroundTask downloadTaxonList;
        private ReactiveAsyncCommand deleteTaxonList = new ReactiveAsyncCommand();

        public ReactiveCommand Select { get; private set; }
        public ReactiveCommand Download { get; private set; }
        public ReactiveCommand DownloadAll { get; private set; }
        public ReactiveCommand Delete { get; private set; }
        public ReactiveCommand Refresh { get; private set; }
        

        #endregion

        public TaxonManagementVM(Container ioc)
            : base(ioc.Resolve<IMessageBus>())
        {            
            Taxa = ioc.Resolve<ITaxonService>();
            Service = ioc.Resolve<IDiversityServiceClient>();
            Background = ioc.Resolve<IBackgroundService>();

            downloadTaxonList = Background.getTaskObject<DownloadTaxonListTask>();

            _IsBusy = Observable.Merge(
                deleteTaxonList.ItemsInflight.Select(count => count > 0),
                downloadTaxonList.ItemsInflight.Select(count => count > 0)
                ).ToProperty(this, x => x.IsBusy, false);

            Select = new ReactiveCommand(_IsBusy.Select(x => !x));
            Select
                .Where(argument => argument is TaxonListVM)
                .Select(argument => argument as TaxonListVM)
                .Subscribe(taxonlist =>
                    {
                        if (!taxonlist.IsDownloading) //Local list
                        {
                            if (!taxonlist.IsSelected)
                            {
                                Taxa.selectTaxonList(taxonlist.Model);
                                taxonlist.IsSelected = true;
                            }
                        }                        
                    });
            Download = new ReactiveCommand(_IsBusy.Select(x => !x));
            Download
                .Where(arg => arg is TaxonListVM)
                .Select(arg => arg as TaxonListVM)
                .Subscribe(taxonlist =>
                        {
                            if (Taxa.getTaxonTableFreeCount() > 0)
                            {                                
                                CurrentPivot = Pivot.Local;
                                
                                Background.startTask<DownloadTaxonListTask>(taxonlist.Model);
                            }
                            else
                                Messenger.SendMessage(new DialogMessage(Messages.DialogType.OK, DiversityResources.TaxonManagement_Message_Error,DiversityResources.TaxonManagement_Message_CantDownload));
                        });
        
            Delete = new ReactiveCommand(_IsBusy.Select(x => !x));
            Delete
                .Where(argument => argument is TaxonListVM)
                .Select(argument => argument as TaxonListVM)
                .Subscribe(taxonlist =>
                    {
                        if (!taxonlist.IsDownloading && deleteTaxonList.CanExecute(taxonlist))
                        {
                            deleteTaxonList.Execute(taxonlist);                                                        
                            taxonlist.IsSelected = false;
                            LocalLists.Remove(taxonlist);
                            RepoLists.Add(taxonlist);
                        }
                    });

            Refresh = new ReactiveCommand(_IsBusy.Select(x => !x));
            Refresh
                .Where(arg => arg is TaxonListVM)
                .Select(arg => arg as TaxonListVM)
                .Subscribe(taxonlist =>
                {
                    if (Delete.CanExecute(taxonlist))
                    {
                        Observable.Zip(
                            deleteTaxonList.ItemsInflight.Skip(1), //most recent value
                            deleteTaxonList.ItemsInflight,//value before that
                            (last,before) => last == 0 && before == 1)
                            .Where(x => x)
                            .Take(1)
                            .Subscribe(_ => Download.Execute(taxonlist));
                        Delete.Execute(taxonlist);
                    }
                });

            DownloadAll = new ReactiveCommand(_IsBusy.Select(x => !x));
            DownloadAll
                .Subscribe(_ =>
                    {
                        if (RepoLists.Any())
                        {
                            downloadTaxonList
                                .AsyncCompletedNotification
                                .TakeWhile(_2 => RepoLists.Any())
                                .Select(_2 => RepoLists.First())
                                .Subscribe(Download.Execute);
                            Download.Execute(RepoLists.First());
                        }
                    });


            var localLists = DistinctStateObservable
                .Take(1)
                .Select(_ => Taxa.getTaxonSelections())                
                .Where(selections => selections != null)
                .Select(selections =>
                    {
                        var currentDownload = downloadTaxonList.CurrentArguments as TaxonList;
                        string downloadingTable = null;
                        if (currentDownload != null)
                            downloadingTable = currentDownload.Table;


                        return selections
                            .Select(selection =>
                            {
                                return new TaxonListVM(new TaxonList() { DisplayText = selection.TableDisplayName, Table = selection.TableName, TaxonomicGroup = selection.TaxonomicGroup })
                                {
                                    IsDownloading = (selection.TableName == downloadingTable),
                                    IsSelected = selection.IsSelected
                                };
                            });
                    })
                .Publish();
            localLists.Connect();

            localLists
                .Where(selections => selections == null || !selections.Any())
                .Subscribe(_ => CurrentPivot = Pivot.Repository);

            LocalLists =            
                localLists              
                .SelectMany(selections => selections)    
                .CreateCollection();

            var repoLists =
               getRepoLists
                   .RegisterAsyncFunction(_ => getRepoListsImpl())
                   .CombineLatest(localLists, (repolists, localselections) =>
                       repolists.Where(repolist => !localselections.Any(selection => selection.Model.Table == repolist.Table)) //Filter Lists that have already been downloaded
                       )
                   .SelectMany(repolists => repolists.Select(list => new TaxonListVM(list) { IsDownloading = false, IsSelected = false }))
                   .Publish();
            repoLists.Connect();

            RepoLists = 
                repoLists
                    .CreateCollection();

            downloadTaxonList
                .AsyncStartedNotification
                .Subscribe(list => startDownload(list as TaxonList));
            downloadTaxonList
                .AsyncCleanupNotification
                .Subscribe(list => abortDownload(list as TaxonList));

            downloadTaxonList
                .AsyncCompletedNotification           
                .Subscribe(downloadedList => 
                    {
                        finishDownload(downloadedList as TaxonList);                       
                    });

            deleteTaxonList
                .RegisterAsyncFunction(arg => deleteListImpl(arg as TaxonListVM));          

            getRepoLists.Execute(null);
            
        }

        private void abortDownload(TaxonList taxonList)
        {
            if (taxonList != null)
            {
                var listVM = LocalLists.Where(vm => vm.Model.Table == taxonList.Table).FirstOrDefault();
                if (listVM != null)
                {
                    LocalLists.Remove(listVM);
                    RepoLists.Add(listVM);
                    listVM.IsDownloading = false;
                    listVM.IsSelected = false;
                }
            }
        }

        private void startDownload(TaxonList list)
        {
            if (list != null)
            {
                var listVM = RepoLists.Where(vm => vm.Model.Table == list.Table).FirstOrDefault();
                if (listVM != null)
                {
                    RepoLists.Remove(listVM);
                    LocalLists.Add(listVM);
                }
                else
                	listVM = LocalLists.Where(vm => vm.Model.Table == list.Table).FirstOrDefault();
                if (listVM != null)
                {                    
                    listVM.IsDownloading = true;                    
                }
            }
        }
     
        private void finishDownload(TaxonList list)
        {
            if (list != null)
            {
                var listVM = LocalLists.Where(vm => vm.Model.Table == list.Table).FirstOrDefault();
                if (listVM != null)
                {
                    listVM.IsDownloading = false;
                    listVM.IsSelected = Taxa.getTaxonSelections()
                                                .Where(sel => sel.TableName == listVM.Model.Table && sel.TaxonomicGroup == listVM.Model.TaxonomicGroup)
                                                .Select(sel => sel.IsSelected)
                                                .FirstOrDefault();
                }
            }
        }

       

        private IEnumerable<TaxonList> getRepoListsImpl()
        {
            return Service.GetTaxonLists().First();
        }

        private TaxonListVM deleteListImpl(TaxonListVM list)
        {
            Taxa.deleteTaxonList(list.Model);
            return list;
        }

        
    }
}
