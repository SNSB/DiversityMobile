﻿using System;
using System.Reactive.Linq;
using ReactiveUI;
using System.Windows;
using System.IO;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using Funq;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using System.Windows.Media.Imaging;

namespace DiversityPhone.ViewModels
{
    public class ViewMapVM : PageVMBase
    {
        private const double SCALEMIN = 0.2;
        private const double SCALEMAX = 3;

        private IMapStorageService MapStorage;
        private ILocationService Location;

        public ReactiveCommand SelectMap { get; private set; }

        public IElementVM<Map> CurrentMap { get { return _CurrentMap.Value; } }
        private ObservableAsPropertyHelper<IElementVM<Map>> _CurrentMap;
        

        private BitmapImage _MapImage;
        public BitmapImage MapImage
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


        private Point? _CurrentLocation = new Point(0.5,0.5);
        public Point? CurrentLocation
        {
            get
            {
                return _CurrentLocation;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.CurrentLocation, ref _CurrentLocation, value);
            }
        }
        

        private double _Scale = 1.0;
        public double Scale 
        {
            get { return _Scale; }
            set
            {                
                if (value > SCALEMAX)
                    value = SCALEMAX;
                else if (value < SCALEMIN)
                    value = SCALEMIN;
                
                this.RaiseAndSetIfChanged(x => x.Scale, ref _Scale, value);                
            }                
        }

        public ViewMapVM(Container ioc)
        {
            MapStorage = ioc.Resolve<IMapStorageService>();
            Location = ioc.Resolve<ILocationService>();

            SelectMap = new ReactiveCommand();
            SelectMap
                .Select(_ => Page.MapManagement)
                .ToMessage();

            this.FirstActivation()
                .Select(_ => Page.MapManagement)
                .ToMessage();


            _CurrentMap = this.ObservableToProperty(Messenger.Listen<IElementVM<Map>>(MessageContracts.VIEW), x => x.CurrentMap);

            
            _CurrentMap
                .SelectMany(vm => Observable.Start(() => MapStorage.loadMap(vm.Model)).TakeUntil(_CurrentMap))                
                .ObserveOnDispatcher()
                .Select(stream => 
                    {
                        var img = new BitmapImage();
                        img.SetSource(stream);
                        return img;
                    })
                .BindTo(this, x => x.MapImage);


            var loc_obs = Location.Location();

            ActivationObservable
                .Where(a => a)
                .Where(_ => CurrentMap != null)  
                .SelectMany(_ => loc_obs.TakeUntil(ActivationObservable.Where(a => !a)))
                .Select(c => CurrentMap.Model.PercentilePositionOnMap(c.Latitude.Value, c.Longitude.Value))
                .Subscribe(c => CurrentLocation = c);

        }
    }
}
