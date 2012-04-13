using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using DiversityPhone.DiversityService;
using ReactiveUI.Xaml;

namespace DiversityPhone.ViewModels
{
    public class ManagedMapVM : ReactiveObject
    {

        #region Commands

        ReactiveCommand Select;

        #endregion

        private string _ServerKey;
        public string ServerKey
        {
            get
            {
                return _ServerKey;
            }
        }

        private string _uri;
        public string Uri
        {
            get { return _uri; }
            set { _uri = value; }
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

        public ManagedMapVM(string serverKey)
        {
            Select = new ReactiveCommand();
            _ServerKey = serverKey;           
        }

        public ManagedMapVM(string serverKey, string uri) :this(serverKey)
        {
            _uri = uri;
            
        }  

    }
}
