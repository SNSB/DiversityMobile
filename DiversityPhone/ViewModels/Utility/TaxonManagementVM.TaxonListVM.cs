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
    public partial class TaxonManagementVM
    {
        public class TaxonListVM : ReactiveObject
        {
            public Icon Icon 
            {
                get
                {
                    return (Icon)Enum.Parse(typeof(Icon), _model.TaxonomicGroup, true);
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

            private bool _IsDownloaded;
            public bool IsDownloaded
            {
                get
                {
                    return _IsDownloaded;
                }
                set
                {
                    this.RaiseAndSetIfChanged(x => x.IsDownloaded, ref _IsDownloaded, value);
                }
            }

            public ICommand Execute { get; private set; }

            TaxonList _model;
            public TaxonList Model { get { return _model; } }

            public TaxonListVM(TaxonList model, ICommand command)
            {
                _model = model;
                Execute = command;
            }
        }
    }
}
