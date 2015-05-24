namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Model;
    using ReactiveUI;
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Windows.Input;

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

        private TaxonList _model;

        public TaxonList Model { get { return _model; } }

        public ICommand Download { get; private set; }

        public ICommand Select { get; private set; }

        public ICommand Delete { get; private set; }

        public ICommand Refresh { get; private set; }

        public TaxonListVM(TaxonList model, TaxonManagementVM parent)
        {
            _model = model;

            _model.ObservableForProperty(x => x.IsSelected)
                .Subscribe(_ => this.RaisePropertyChanged(x => x.IsSelected));
            Predicate<TaxonListVM> notDownloading = x => !x.IsDownloading;
            IObservable<Unit> downloadingChanged = this.ObservableForProperty(x => x.IsDownloading).Value().Select(_ => Unit.Default);

            Download = parent.Download.Relay<TaxonListVM>(
                canExecute: notDownloading,
                mapParameter: _ => this,
                canExecuteChanged: downloadingChanged);
            Select = parent.Select.Relay<TaxonListVM>(
                canExecute: notDownloading,
                mapParameter: _ => this,
                canExecuteChanged: downloadingChanged);
            Delete = parent.Delete.Relay<TaxonListVM>(
                canExecute: notDownloading,
                mapParameter: _ => this,
                canExecuteChanged: downloadingChanged);
            Refresh = parent.Refresh.Relay<TaxonListVM>(
                canExecute: notDownloading,
                mapParameter: _ => this,
                canExecuteChanged: downloadingChanged);
        }
    }
}