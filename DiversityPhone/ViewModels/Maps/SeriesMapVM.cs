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
using Funq;
using DiversityPhone.Services;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using System.Collections.Generic;

namespace DiversityPhone.ViewModels.Maps
{
    public class SeriesMapVM : ReactiveObject
    {
        private IMessageBus Messenger;
        private IFieldDataService Storage;

        private Map _CurrentMap;
        public Map CurrentMap
        {
            get
            {
                return _CurrentMap;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.CurrentMap, ref _CurrentMap, value);
            }
        }

        public IElementVM<EventSeries> CurrentSeries { get { return _CurrentSeries.Value; } }
        private ObservableAsPropertyHelper<IElementVM<EventSeries>> _CurrentSeries;
        
        public SeriesMapVM(Container ioc)
        {
            Messenger = ioc.Resolve<IMessageBus>();
            Storage = ioc.Resolve<IFieldDataService>();

            _CurrentSeries = this.ObservableToProperty(Messenger.Listen<IElementVM<EventSeries>>(MessageContracts.MAPS), x => x.CurrentSeries);
        }

        private IEnumerable<Point> seriesPointsOnMap(EventSeries es, Map m)
        {
            foreach (var c in Storage.getGeoPointsForSeries(es.SeriesID.Value))
            {
                var p = m.PercentilePositionOnMap(c);
                if (p.HasValue)
                    yield return p.Value;
                    
            }
        }
    }
}
