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
    public class EditPropertyVM:ReactiveObject
    {
        private IList<IDisposable> _subscriptions;

        #region Services
        private IMessageBus _messenger;
        IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand Edit { get; private set; }
        public ReactiveCommand Delete { get; private set; }
        #endregion

        #region Properties
        private CollectionEventProperty _Model;
        public CollectionEventProperty Model
        {
            get { return _Model; }
            set { this.RaiseAndSetIfChanged(x => x.Model, ref _Model, value);
            }
        }


        private IList<Property> _Properties = null;
        public IList<Property> Properties
        {
            get
            {
                return _Properties?? (_Properties = _storage.getAllProperties());
            }
        }

        private Property _SelectedProperty;
        public Property SelectedProperty
        {
            get { return _SelectedProperty; }
            set { this.RaiseAndSetIfChanged(x => x.SelectedProperty, ref _SelectedProperty, value); }
        }

        private IList<PropertyName> _PropertyNames = null; //Vorläufige Implementierung Suchfilter benötigt
        public IList<PropertyName> PropertyNames
        {
            get
            {
                if(this.SelectedProperty==null)
                    throw new NullReferenceException();
                return _PropertyNames?? (_PropertyNames = _storage.getPropertyNames(this.SelectedProperty));
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
        {

            _messenger = messenger;
            _storage = storage;


            var canSave1 = this.ObservableForProperty(x => x.SelectedProperty)//1.Bedingung
                .Select(change => change.Value!=null)
                .StartWith(false);

            
            var canSave2 = this.ObservableForProperty(x => x.SelectedPropertyName)//2.Bedingung
                .Select(change => change.Value!=null)
                .StartWith(false);

            var canSave=this.ObservableToProperty(canSave1.And(canSave2));//wie sieht ein mögliches Pattern aus?

            _subscriptions = new List<IDisposable>()
            {
                (Save = new ReactiveCommand(canSave))               
                    .Subscribe(_ => executeSave()),

                (Edit = new ReactiveCommand())
                    .Subscribe(_ => enableEdit()),

                (Delete = new ReactiveCommand())
                    .Subscribe(_ => delete()),

                _messenger.Listen<CollectionEventProperty>(MessageContracts.EDIT)
                    .Subscribe(cep => updateView(cep))
            };
        }



        private void executeSave()
        {
            updateModel();
            _messenger.SendMessage<CollectionEventProperty>(Model, MessageContracts.SAVE);
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }


        private void enableEdit()
        {
        }

        private void delete()
        {
            _messenger.SendMessage<CollectionEventProperty>(Model, MessageContracts.DELETE);
            _messenger.SendMessage<Message>(Message.NavigateBack);
        }

        private void updateModel()
        {
            Model.PropertyID=this.SelectedProperty.PropertyID;
            Model.DisplayText=this.SelectedProperty.DisplayText;
            Model.PropertyUri=this.SelectedPropertyName.PropertyUri;
        }

        private void updateView(CollectionEventProperty cep)
        {
            this.Model = cep;
            this.SelectedProperty = _storage.getPropertyByID(cep.PropertyID);
            this._Properties=_storage.getAllProperties();
            this.SelectedPropertyName=_storage.getPropertyNameByURI(cep.PropertyUri);
            this._PropertyNames=_storage.getPropertyNames(this.SelectedProperty);         
        }
    }
}
