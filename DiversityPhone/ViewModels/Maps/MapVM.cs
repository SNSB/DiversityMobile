using DiversityPhone.Model;
using ReactiveUI;
using System;

namespace DiversityPhone.ViewModels
{
    public class MapVM : ElementVMBase<Map>
    {
        private string _ServerKey;

        public string ServerKey
        {
            get
            {
                return (Model != null) ? Model.ServerKey : _ServerKey;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.ServerKey, ref _ServerKey, value);
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

        public MapVM(Map model)
            : base(model)
        {
        }

        public void SetModel(Map model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (Model != null)
                throw new InvalidOperationException("Model already set");

            Model = model;
            IsDownloading = false;

            this.RaisePropertyChanged(x => x.Model);
            this.RaisePropertyChanged(x => x.Description);
        }

        public override string Description
        {
            get { return (Model != null) ? Model.Description : ServerKey; }
        }

        public override Icon Icon
        {
            get { return ViewModels.Icon.Map; }
        }
    }
}