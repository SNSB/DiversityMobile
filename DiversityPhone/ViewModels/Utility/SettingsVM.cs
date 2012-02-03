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
using DiversityPhone.Services;
using DiversityPhone.Model;
using System.Reactive.Linq;
using ReactiveUI.Xaml;
using ReactiveUI;

namespace DiversityPhone.ViewModels.Utility
{
    public class SettingsVM : PageViewModel
    {
        public enum Pivots
        {
            Login,
            Repository,
            Preferences
        }

        SettingsService _settings;

        public ReactiveCommand Save { get; private set; }



        private Pivots _CurrentPivot;
        public Pivots CurrentPivot
        {
            get
            {
                return _CurrentPivot;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.CurrentPivot, ref _CurrentPivot, value);
            }
        }
        
        

        public AppSettings Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<AppSettings> _Model;
        
        
        public SettingsVM(SettingsService set)            
        {
            _settings = set;

            Save = new ReactiveCommand(CanSave());


        }

        private IObservable<bool> CanSave()
        {
            var model = this.ObservableForProperty(x => x.Model)
                            .Select(change => change.Value)
                            .Where(x => x != null);


            return null;
        }
    }
}
