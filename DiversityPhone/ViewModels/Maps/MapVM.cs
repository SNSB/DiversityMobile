using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using DiversityPhone.DiversityService;
using ReactiveUI.Xaml;
using DiversityPhone.Model;

namespace DiversityPhone.ViewModels
{
    public class MapVM : ElementVMBase<Map>
    {
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


        public override string Description
        {
            get { return Model.Description; }
        }

        public override Icon Icon
        {
            get { return ViewModels.Icon.Map; }
        }
    }
}
