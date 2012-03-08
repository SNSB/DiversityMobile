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
using DiversityPhone.DiversityService;
using ReactiveUI.Xaml;

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

        private IList<TaxonSelection> _TaxonSelections;
        public ReactiveCollection<TaxonListVM> LocalLists { get; private set; }
        public ReactiveCollection<TaxonListVM> RepoLists { get; private set; } 
     
        private ReactiveAsyncCommand getRepoLists = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand downloadTaxonList = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand deleteTaxonList = new ReactiveAsyncCommand();

        public ReactiveCommand SelectOrDownload { get; private set; }
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

            SelectOrDownload = new ReactiveCommand(_IsBusy.Select(x => !x));
            SelectOrDownload
                .Where(argument => argument is TaxonListVM)
                .Select(argument => argument as TaxonListVM)
                .Subscribe(taxonlist =>
                    {
                        if (taxonlist.IsDownloaded) //Local list
                        {
                            if (!taxonlist.IsSelected)
                            {
                                Storage.selectTaxonList(taxonlist.Model);
                                taxonlist.IsSelected = true;
                            }
                        }
                        else //Repo list
                        {
                            if (Storage.getTaxonTableFreeCount() > 0)
                            {
                                if(downloadTaxonList.CanExecute(taxonlist))
                                    downloadTaxonList.Execute(taxonlist);
                            }
                            else
                                Messenger.SendMessage(new DialogMessage(Messages.DialogType.OK, "Error", "Can't download more than 10 Taxon tables."));
                        }
                    });
            Delete = new ReactiveCommand(_IsBusy.Select(x => !x));
            Delete
                .Where(argument => argument is TaxonListVM)
                .Select(argument => argument as TaxonListVM)
                .Subscribe(taxonlist =>
                    {
                        if (taxonlist.IsDownloaded && deleteTaxonList.CanExecute(taxonlist.Model))
                        {
                            deleteTaxonList.Execute(taxonlist.Model);                            
                            taxonlist.IsDownloaded = false;
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
                    return new TaxonListVM(new TaxonList() { DisplayText = selection.TableDisplayName, Table = selection.TableName, TaxonomicGroup = selection.TaxonomicGroup }, SelectOrDownload)
                    { 
                        IsDownloaded = true, 
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
                    .SelectMany(repolists => repolists.Select(list => new TaxonListVM(list, SelectOrDownload) { IsDownloaded = false, IsSelected = false }))
                    .CreateCollection();

            deleteTaxonList
                .RegisterAsyncAction(arg => deleteListImpl(arg as TaxonList));

            getRepoLists.Execute(null);                       
        }

        private IEnumerable<TaxonList> getRepoListsImpl()
        {
            return Service.GetTaxonLists().First();
        }

        private void deleteListImpl(TaxonList list)
        {
            Storage.deleteTaxonList(list);
        }

        
    }
}
