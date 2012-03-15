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

namespace DiversityPhone.ViewModels
{
    public partial class TaxonManagementVM : PageViewModel
    {
        #region Services
        private IOfflineStorage Storage { get; set; }
        private IDiversityServiceClient Service { get; set; }
        #endregion

        #region Properties

        public bool IsBusy { get { return _IsBusy.Value; } }
        private ObservableAsPropertyHelper<bool> _IsBusy;       

        private IList<Client.TaxonSelection> _TaxonSelections;
        public ObservableCollection<TaxonListVM> LocalLists { get; private set; }
        public ObservableCollection<TaxonListVM> RepoLists { get; private set; } 
     
        private ReactiveAsyncCommand getRepoLists = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand downloadTaxonList = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand deleteTaxonList = new ReactiveAsyncCommand();

        public ReactiveCommand Select { get; private set; }
        public ReactiveCommand Download { get; private set; }
        public ReactiveCommand Delete { get; private set; }
        

        #endregion

        public TaxonManagementVM(IMessageBus messenger, IOfflineStorage storage, IDiversityServiceClient service)
            : base(messenger)
        {
            Storage = storage;
            Service = service;

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
                                Storage.selectTaxonList(taxonlist.Model);
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
                            if (Storage.getTaxonTableFreeCount() > 0)
                            {
                                RepoLists.Remove(taxonlist);
                                taxonlist.IsDownloading = true;
                                LocalLists.Add(taxonlist);

                                if (downloadTaxonList.CanExecute(taxonlist))
                                    downloadTaxonList.Execute(taxonlist);
                            }
                            else
                                Messenger.SendMessage(new DialogMessage(Messages.DialogType.OK, "Error", "Can't download more than 10 Taxon tables."));
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


            var taxonSelections = DistinctStateObservable
                .Take(1)
                .Select(_ => Storage.getTaxonSelections())
                .Publish();
            taxonSelections.Connect();


            LocalLists =            
                taxonSelections
                .SelectMany(selections => selections)
                .Select(selection => 
                { 
                    return new TaxonListVM(new TaxonList() { DisplayText = selection.TableDisplayName, Table = selection.TableName, TaxonomicGroup = selection.TaxonomicGroup })
                    { 
                        IsDownloading = false, 
                        IsSelected = selection.IsSelected
                    };
                })
                .CreateCollection();

            RepoLists = 
                getRepoLists
                    .RegisterAsyncFunction(_ => getRepoListsImpl())
                    .CombineLatest(taxonSelections, (repolists, localselections) =>
                        repolists.Where(repolist => !localselections.Any(selection => selection.TableName == repolist.Table)) //Filter Lists that have already been downloaded
                        )
                    .SelectMany(repolists => repolists.Select(list => new TaxonListVM(list) { IsDownloading = false, IsSelected = false }))
                    .CreateCollection();

            downloadTaxonList
                .RegisterAsyncFunction(arg => downloadTaxonListImpl(arg as TaxonListVM))           
                .Subscribe(downloadedList => 
                    {
                        downloadedList.IsDownloading = false;
                        downloadedList.IsSelected = Storage.getTaxonSelections()
                                                    .Where(sel => sel.TableName == downloadedList.Model.Table && sel.TaxonomicGroup == downloadedList.Model.TaxonomicGroup)
                                                    .Select(sel => sel.IsSelected)
                                                    .FirstOrDefault();                        
                    });

            deleteTaxonList
                .RegisterAsyncFunction(arg => deleteListImpl(arg as TaxonListVM));               

            getRepoLists.Execute(null);                       
        }

        private TaxonListVM downloadTaxonListImpl(TaxonListVM taxonList)
        {            
            Service.DownloadTaxonListChunked(taxonList.Model)
                .ForEach(chunk => Storage.addTaxonNames(chunk, taxonList.Model));           
            
            return taxonList;        
        }

        private IEnumerable<TaxonList> getRepoListsImpl()
        {
            return Service.GetTaxonLists().First();
        }

        private TaxonListVM deleteListImpl(TaxonListVM list)
        {
            Storage.deleteTaxonList(list.Model);
            return list;
        }

        
    }
}
