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
    public class EditPropertyVM : EditElementPageVMBase<CollectionEventProperty>
    {
        

        #region Services        
        private IVocabularyService Vocabulary { get; set; }
        private IFieldDataService Storage { get; set; }   
        #endregion        

        #region Properties       

        public ListSelectionHelper<Property> Properties { get; private set; }

        public ListSelectionHelper<PropertyName> Values { get; private set; }
        #endregion

        private ReactiveAsyncCommand getProperties = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getValues = new ReactiveAsyncCommand();


        public EditPropertyVM(Container ioc)      
            :  base(false)
        {
            Vocabulary = ioc.Resolve<IVocabularyService>();
            Storage = ioc.Resolve<IFieldDataService>();

            Properties = new ListSelectionHelper<Property>();
            ValidModel                
                .Subscribe(getProperties.Execute);
            getProperties.RegisterAsyncFunction(arg => getPropertiesImpl(arg as CollectionEventProperty))
                .Merge(ValidModel.Select(_ => Enumerable.Empty<Property>() as IEnumerable<Property>))
                .Select(props => props.ToList() as IList<Property>)
                .Subscribe(Properties);

            Values = new ListSelectionHelper<PropertyName>();
            Properties                
                .Subscribe(getValues.Execute);
            getValues.RegisterAsyncFunction(prop => getValuesImpl(prop as Property))
                .Merge(Properties.Select(_ => new List<PropertyName>() as IList<PropertyName>))
                .Subscribe(Values);


            ValidModel
                .CombineLatest(Properties.ItemsObservable, (m, p) => p.FirstOrDefault(prop => prop.PropertyID == m.PropertyID))
                .BindTo(Properties, x => x.SelectedItem);

            ValidModel
                .CombineLatest(Values.ItemsObservable, (m, p) => p.FirstOrDefault(prop => prop.PropertyUri == m.PropertyUri))
                .BindTo(Values, x => x.SelectedItem);


            CanSaveObs()
                .SubscribeOnDispatcher()
                .Subscribe(_CanSaveSubject.OnNext);
        }

        private IEnumerable<Property> getPropertiesImpl(CollectionEventProperty cep)
        {
            var otherCEPs = Storage.getPropertiesForEvent(cep.EventID).ToDictionary(x => x.PropertyID);
            otherCEPs.Remove(cep.PropertyID);

            var props = Vocabulary.getAllProperties();

            return props.Where(prop => !otherCEPs.ContainsKey(prop.PropertyID));
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


            var valueSelected = Values
                 .Select(x => x != null)
                 .StartWith(false);

            var isnew = StateObservable.Select(s => s.Context == null).StartWith(false);

            return Extensions.BooleanAnd(propSelected, valueSelected, isnew);
        }         


        protected override void UpdateModel()
        {           
            Current.Model.PropertyID = Properties.SelectedItem.PropertyID;
            Current.Model.PropertyUri = Values.SelectedItem.PropertyUri;
            Current.Model.DisplayText = Values.SelectedItem.DisplayText;
        }

        protected override CollectionEventProperty ModelFromState(PageState s)
        {
            if (s.Referrer != null)
            {
                int evID;
                if (int.TryParse(s.Referrer, out evID))
                {
                    int propID;
                    if (s.Context != null && int.TryParse(s.Context, out propID))
                    {
                        return Storage.getPropertyByID(evID, propID);
                    }
                    else
                        return new CollectionEventProperty()
                        {
                            EventID = evID
                        };
                         
                }
            }
            return null;
        }

        protected override ElementVMBase<CollectionEventProperty> ViewModelFromModel(CollectionEventProperty model)
        {
            return new PropertyVM(Messenger, model, Page.Current);
        }
    }
}
