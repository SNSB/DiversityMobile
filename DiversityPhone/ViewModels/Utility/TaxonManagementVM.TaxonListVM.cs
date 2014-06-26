namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Model;
    using ReactiveUI;
    using System;

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
                return _model.TableDisplayName;
            }
        }
        
        public bool IsSelected 
        {
            get
            {
                return Model.IsSelected;
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

            _model.ObservableForProperty(x => x.IsSelected)
                .Subscribe(_ => this.RaisePropertyChanged(x => x.IsSelected));
        }        
    }
}
