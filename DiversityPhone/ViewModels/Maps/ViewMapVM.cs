using System;
using System.Reactive.Linq;
using ReactiveUI;
using System.Windows;
using System.IO;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using Funq;

namespace DiversityPhone.ViewModels
{
    public class ViewMapVM : PageVMBase
    {
        private const double SCALEMIN = 0.2;
        private const double SCALEMAX = 3;

        public IElementVM<Map> CurrentMap { get { return _CurrentMap.Value; } }
        private ObservableAsPropertyHelper<IElementVM<Map>> _CurrentMap;
        

        private MemoryStream _MapImage;
        public MemoryStream MapImage
        {
            get
            {
                return _MapImage;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.MapImage, ref _MapImage, value);
            }
        }


        private int _MapHeight;
        public int MapHeight
        {
            get
            {
                return _MapHeight;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.MapHeight, ref _MapHeight, value);
            }
        }


        private int _MapWidth;

        public int MapWidth
        {
            get
            {
                return _MapWidth;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.MapWidth, ref _MapWidth, value);
            }
        }    
        
        

        private double _Scale = 1.0;
        public double Scale 
        {
            get { return _Scale; }
            set
            {
                if (_Scale != value)
                {
                    if (value > SCALEMAX)
                        value = SCALEMAX;
                    else if (value < SCALEMIN)
                        value = SCALEMIN;

                    _Scale = value;
                    this.RaisePropertyChanged(x => x.Scale);
                }
            }                
        }

        public ViewMapVM(Container ioc)
        {
            _CurrentMap = this.ObservableToProperty(Messenger.Listen<IElementVM<Map>>(MessageContracts.VIEW), x => x.CurrentMap);

            
        }
    }
}
