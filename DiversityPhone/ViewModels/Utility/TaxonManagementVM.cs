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
    public class TaxonManagementVM : PageViewModel
    {
        #region Services
        private IOfflineStorage Storage { get; set; }
        private IDiversityServiceClient Service { get; set; }
        #endregion

        #region Properties

        public bool IsRefreshing { get { return _IsRefreshing.Value; } }
        private ObservableAsPropertyHelper<bool> _IsRefreshing;

        public bool IsDownloading { get { return _IsDownloading.Value; } }
        private ObservableAsPropertyHelper<bool> _IsDownloading;

        public IEnumerable<TaxonSelectionVM> TaxonLists { get { return _TaxonLists.Value; } }
        private ObservableAsPropertyHelper<IEnumerable<TaxonSelectionVM>> _TaxonLists;
        


        

        #endregion

        public TaxonManagementVM(IMessageBus messenger, IOfflineStorage storage, IDiversityServiceClient service)
            : base(messenger)
        {
            Storage = storage;
            Service = service;            

            _TaxonLists = DistinctStateObservable
                .SelectMany(_ => Service.GetTaxonLists())                
                .Select(taxlists =>
                    {
                        var selections =
                            Storage.getTaxonSelections()
                                .OrderBy(tls => tls.TableName);
                        var availableLists =
                            taxlists
                                .OrderBy(tl => tl.Table);

                        return from tl in availableLists
                               join sel in selections on tl.Table equals sel.TableName into outer
                               from sel in outer.DefaultIfEmpty()
                               orderby tl.DisplayText
                               select new TaxonSelectionVM(this, tl, sel);
                    })
                .ToProperty(this, vm => vm.TaxonLists);
                             
                
                
                
        }

        public class TaxonSelectionVM
        {
            TaxonManagementVM _parent;

            TaxonList _list;
            TaxonSelection _selection;
            public TaxonSelectionVM(TaxonManagementVM parent, TaxonList list, TaxonSelection selection)
            {
                _parent = parent;
                _list = list;
                _selection = selection;   
             

            }

            public string Name { get { return _list.DisplayText; } }

            public Icon Icon
            {
                get
                {
                    try
                    {
                        return (Icon)Enum.Parse(typeof(Icon), _list.TaxonomicGroup, true);
                    }
                    catch
                    {
                        return Icon.None;
                    }
                }
            }

            public ReactiveCommand Download { get; private set; }
            public ReactiveCommand Select { get; private set; }


            public bool IsDownloaded { get { return _IsDownloaded.Value; } }
            private ObservableAsPropertyHelper<bool> _IsDownloaded;


            public bool IsSelected { get { return _IsSelected.Value; } }
            private ObservableAsPropertyHelper<bool> _IsSelected;
        
        

        }        
    }
}
