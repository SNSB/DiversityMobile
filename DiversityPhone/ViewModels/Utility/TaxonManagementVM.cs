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

namespace DiversityPhone.ViewModels
{
    public class TaxonManagementVM : PageViewModel
    {
        #region Services
        private IOfflineStorage Storage { get; set; }
        private DiversityService.IDiversityService Service { get; set; }
        #endregion

        #region Properties

        public bool IsRefreshing { get { return _IsRefreshing.Value; } }
        private ObservableAsPropertyHelper<bool> _IsRefreshing;

        public bool IsDownloading { get { return _IsDownloading.Value; } }
        private ObservableAsPropertyHelper<bool> _IsDownloading;

        public IEnumerable<TaxonSelectionVM> TaxonLists { get { return _TaxonLists.Value; } }
        private ObservableAsPropertyHelper<IEnumerable<TaxonSelectionVM>> _TaxonLists;
        


        

        #endregion

        public TaxonManagementVM(IMessageBus messenger, IOfflineStorage storage, DiversityService.IDiversityService service)
            : base(messenger)
        {
            Storage = storage;
            Service = service;

            var taxonListsForUser = Observable.FromAsyncPattern<DiversityService.UserProfile,IEnumerable<DiversityService.TaxonList>>(Service.BeginGetTaxonListsForUser, Service.EndGetTaxonListsForUser);

            _TaxonLists = DistinctStateObservable
                .Select(_ => taxonListsForUser(new DiversityService.UserProfile() { LoginName = "rollinger" }))
                .SelectMany(id => id)
                .Select(taxlists =>
                    {
                        var selections = 
                            Storage.getTaxonSelections()
                                .OrderBy( tls => tls.TableName);
                        var availableLists = 
                            taxlists
                                .OrderBy(tl => tl.Table);
                        
                        


                        return from tl in taxlists
                               join sel in selections on tl.Table equals sel.TableName into outer
                               from sel in outer.DefaultIfEmpty()
                               select new TaxonSelectionVM(this, tl, sel);
                    })
                .ToProperty(this, vm => vm.TaxonLists);
                             
                
                
                
        }

        public class TaxonSelectionVM
        {


            TaxonManagementVM _parent;
            public TaxonSelectionVM(TaxonManagementVM parent, DiversityService.TaxonList list, TaxonSelection selection)
            {
                _parent = parent;
            }


        }
    }
}
