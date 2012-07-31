using System;
using System.Net;
using System.Reactive.Linq;
using ReactiveUI;
using System.Linq;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;
using DiversityPhone.Services;
using Funq;

namespace DiversityPhone.ViewModels
{
    public class EditPropertyVM : EditPageVMBase<EventProperty>
    {      

        #region Services        
        private IVocabularyService Vocabulary { get; set; }
        private IFieldDataService Storage { get; set; }   
        #endregion        

        #region Properties    
   

        public bool IsNew { get { return _IsNew.Value; } }
        private ObservableAsPropertyHelper<bool> _IsNew;

        private Property NoProperty = new Property() { DisplayText = DiversityResources.Setup_Item_PleaseChoose };
        public ListSelectionHelper<Property> Properties { get; private set; }

        private PropertyName NoValue = new PropertyName() { DisplayText = DiversityResources.Setup_Item_PleaseChoose };
        public ListSelectionHelper<PropertyName> Values { get; private set; }
        #endregion

        private ReactiveAsyncCommand getProperties = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getValues = new ReactiveAsyncCommand();


        public EditPropertyVM(Container ioc)                  
        {
            Vocabulary = ioc.Resolve<IVocabularyService>();
            Storage = ioc.Resolve<IFieldDataService>();

            _IsNew = this.ObservableToProperty(CurrentModelObservable.Select(m => m.IsNew()), x => x.IsNew, false);

            CurrentModelObservable
                .Subscribe(getProperties.Execute);
            Properties = new ListSelectionHelper<Property>();            
            getProperties.RegisterAsyncFunction(prop => getPropertiesImpl(prop as EventProperty))                
                .Select(props => props.ToList() as IList<Property>)
                .Subscribe(Properties);

            Values = new ListSelectionHelper<PropertyName>();
            Properties                
                .Subscribe(getValues.Execute);
            getValues.RegisterAsyncFunction(prop => getValuesImpl(prop as Property))                
                .Subscribe(Values);


            CurrentModelObservable
                .CombineLatest(Properties.ItemsObservable, (m, p) => p.FirstOrDefault(prop => prop.PropertyID == m.PropertyID))
                .BindTo(Properties, x => x.SelectedItem);

            CurrentModelObservable
                .CombineLatest(Values.ItemsObservable, (m, p) => p.FirstOrDefault(prop => prop.PropertyUri == m.PropertyUri))
                .BindTo(Values, x => x.SelectedItem);


            CanSaveObs()
                .Subscribe(CanSaveSubject.OnNext);
        }

        private IEnumerable<Property> getPropertiesImpl(EventProperty cep)
        {
            var props = Vocabulary.getAllProperties();
            if (cep.IsNew()) //All remaining Properties
            {
                var otherCEPs = Storage.getPropertiesForEvent(cep.EventID).ToDictionary(x => x.PropertyID);
                return props.Where(prop => !otherCEPs.ContainsKey(prop.PropertyID));
            }
            else //Only this Property
            {
                return props.Where(prop => prop.PropertyID == cep.PropertyID);
            }
        }

        private IList<PropertyName> getValuesImpl(Property p)
        {
            return Vocabulary.getPropertyNames(p);           
        }

        private IObservable<bool> CanSaveObs()
        {            
            var propSelected = Properties
                .Select(x => x!=null)
                .StartWith(false);

            var valuesLoaded = getValues.ItemsInflight.Select(items => items == 0).StartWith(false);


            var valueSelected = Values
                 .Select(x => x != null)
                 .StartWith(false);

            return Extensions.BooleanAnd(propSelected, valueSelected, valuesLoaded);
        }         


        protected override void UpdateModel()
        {           
            Current.Model.PropertyID = Properties.SelectedItem.PropertyID;
            Current.Model.PropertyUri = Values.SelectedItem.PropertyUri;
            Current.Model.DisplayText = Values.SelectedItem.DisplayText;
        }
    }
}
