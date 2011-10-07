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
using System.Reactive.Linq;
using System.Collections.Generic;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using ReactiveUI.Xaml;

namespace DiversityPhone.ViewModels
{
    public class ViewCSVM:ReactiveObject
    {
        IMessageBus _messenger;
        IOfflineStorage _storage;
        IList<IDisposable> _subscriptions;

        private Specimen Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<Specimen> _Model;

        public ReactiveCommand AddSubunit { get; private set; }
        
        public ViewCSVM(IMessageBus messenger,IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;

            _Model = _messenger.Listen<Specimen>(MessageContracts.SELECT)
                .ToProperty(this, x => x.Model);
                    

            _subscriptions = new List<IDisposable>()
            {
                
            };
        }

        private void selectSpecimen(Specimen spec)
        {

        }
    }
}
