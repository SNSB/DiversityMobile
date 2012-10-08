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
using System.Reactive.Concurrency;

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


        public EditPropertyVM(Container ioc)                  
        {
            Vocabulary = ioc.Resolve<IVocabularyService>();
            Storage = ioc.Resolve<IFieldDataService>();

            _IsNew = this.ObservableToProperty(CurrentModelObservable.Select(m => m.IsNew()), x => x.IsNew, false);
            
            
            Properties = new ListSelectionHelper<Property>();            
            CurrentModelObservable
                .Select(evprop => 
                    {
                        var isNew = evprop.IsNew();

                        return
                        Vocabulary
                            .getAllProperties()
                            .ToObservable(Scheduler.ThreadPool)
                            .Where(p => isNew || p.PropertyID == evprop.PropertyID) //Cannot change property type only value
                            .TakeUntil(CurrentModelObservable)
                            .ObserveOnDispatcher()  
                            .StartWith(NoProperty)
                            .CreateCollection();
                    })      
                //Select first property automatically after adding it
                .Do(props => props
                    .ItemsAdded
                    .Where(item => item != NoProperty)
                    .Take(1)
                    .Subscribe(item => Properties.SelectedItem = item)
                    )
                .Select(coll => coll as IList<Property>)
                .Subscribe(Properties);

            Values = new ListSelectionHelper<PropertyName>();
            Properties                  
                .Select(prop => 
                    {
                        var values = 
                            (prop == null || prop == NoProperty) 
                            ? Observable.Empty<PropertyName>() 
                            : Vocabulary
                                .getPropertyNames(prop.PropertyID)
                                .ToObservable(Scheduler.ThreadPool)
                                .TakeUntil(Properties);

                        return
                            values
                            .StartWith(NoValue)
                            .ObserveOnDispatcher()
                            .CreateCollection();
                    })
                    //Reselect value that was selected
                    .Do(values => 
                        values
                        .ItemsAdded
                        .Where(item => item.PropertyUri == Current.Model.PropertyUri)
                        .Subscribe(value => Values.SelectedItem = value)
                        )
                    .Select(coll => coll as IList<PropertyName>)
                .Subscribe(Values);
          

            CanSaveObs()
                .Subscribe(CanSaveSubject.OnNext);
        }

       
        private IObservable<bool> CanSaveObs()
        {            
            var propSelected = Properties
                .Select(x => x!=NoProperty)
                .StartWith(false);

            var valueSelected = Values
                 .Select(x => x != NoValue)
                 .StartWith(false);

            return Extensions.BooleanAnd(propSelected, valueSelected);
        }         


        protected override void UpdateModel()
        {           
            Current.Model.PropertyID = Properties.SelectedItem.PropertyID;
            Current.Model.PropertyUri = Values.SelectedItem.PropertyUri;
            Current.Model.DisplayText = Values.SelectedItem.DisplayText;
        }
    }
}
