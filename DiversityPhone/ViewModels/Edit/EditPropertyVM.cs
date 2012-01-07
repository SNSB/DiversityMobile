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
using System.Reactive.Linq;
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class EditPropertyVM : PageViewModel
    {
        

        #region Services        
        IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand Save { get; private set; }        
        public ReactiveCommand Delete { get; private set; }
        #endregion

        #region Properties
        private ObservableAsPropertyHelper<EventVM> _Event;
        public EventVM Event 
        { 
            get
            {
                return _Event.Value;
            } 
        }

        private ObservableAsPropertyHelper<IList<Property>> _Properties;
        public IList<Property> Properties
        {
            get
            {
                return _Properties.Value;
            }
        }

        private Property _SelectedProperty;
        public Property SelectedProperty
        {
            get { return _SelectedProperty; }
            set { this.RaiseAndSetIfChanged(x => x.SelectedProperty, ref _SelectedProperty, value); }
        }

        private ObservableAsPropertyHelper<IList<PropertyName>> _PropertyNames; 
        public IList<PropertyName> PropertyNames
        {
            get
            {
                return _PropertyNames.Value;
            }
        }

        private PropertyName _SelectedPropertyName;
        public PropertyName SelectedPropertyName
        {
            get { return _SelectedPropertyName; }
            set { this.RaiseAndSetIfChanged(x => x.SelectedPropertyName, ref _SelectedPropertyName, value); }
        }

        #endregion


        public EditPropertyVM(IMessageBus messenger, IOfflineStorage storage)
            : base(messenger)
        {

            
            _storage = storage;

            _Properties = StateObservable
                .Select(_ => _storage.getAllProperties())
                .ToProperty(this, vm => vm.Properties);
            _PropertyNames = this.ObservableForProperty(vm => vm.SelectedProperty)
                .Select(prop => _storage.getPropertyNames(prop.Value))
                .ToProperty(this, vm => vm.PropertyNames);



            Save = new ReactiveCommand(cansave());
            var saveMessageSource = Save
                .Select(_ => 
                    new CollectionEventProperty()
                        {
                            EventID = Event.Model.EventID,
                            PropertyID = SelectedProperty.PropertyID,
                            PropertyUri = SelectedPropertyName.PropertyUri
                        }
                    );
            Messenger.RegisterMessageSource(saveMessageSource,MessageContracts.SAVE);
            Messenger.RegisterMessageSource(saveMessageSource.Select(_=>Message.NavigateBack));
            
        }

        IObservable<bool> cansave()
        {
            var canSave1 = this.ObservableForProperty(x => x.SelectedProperty)//1.Bedingung
            .Select(change => change.Value != null)
            .StartWith(false);


            var canSave2 = this.ObservableForProperty(x => x.SelectedPropertyName)//2.Bedingung
                .Select(change => change.Value != null)
                .StartWith(false);

            return Extensions.BooleanAnd(canSave1, canSave2);
        }        
                        

        private Event EventFromState(PageState s)
        {
            if (s.Context != null)
            {
                int id;
                if (int.TryParse(s.Context, out id))
                {
                    return _storage.getEventByID(id);
                }                
            }    
            return null;
        }


    }
}
