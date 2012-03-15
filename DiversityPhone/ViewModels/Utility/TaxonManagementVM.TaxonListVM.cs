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
using DiversityPhone.DiversityService;

namespace DiversityPhone.ViewModels
{   
    public class TaxonListVM : ReactiveObject
    {
        public string TaxonomicGroup 
        {
            get
            {
                return _model.TaxonomicGroup;
            }
        }

        public string DisplayText
        {
            get
            {
                return _model.DisplayText;
            }
        }


        private bool _IsSelected;
        public bool IsSelected 
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.IsSelected, ref _IsSelected, value);
            }
        }

        private bool _IsDownloading;
        public bool IsDownloading
        {
            get
            {
                return _IsDownloading;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.IsDownloading, ref _IsDownloading, value);
            }
        }       

        TaxonList _model;
        public TaxonList Model { get { return _model; } }

        public TaxonListVM(TaxonList model)
        {
            _model = model;            
        }        
    }
}
