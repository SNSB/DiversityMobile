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

namespace DiversityPhone.ViewModels
{
    public class EditPropertyVM : EditElementPageVMBase<CollectionEventProperty>
    {
        

        #region Services        
        private IVocabularyService Vocabulary { get; set; }        
        #endregion        

        #region Properties       

        public ListSelectionHelper<Property> Properties { get; private set; }

        public ListSelectionHelper<PropertyName> Values { get; private set; }
        #endregion


        public EditPropertyVM(IVocabularyService voc)            
        {
            Vocabulary = voc;
            

            Properties = new ListSelectionHelper<Property>();
            DistinctStateObservable
                .Select(_ => Vocabulary.getAllProperties())
                .Subscribe(Properties);

            Values = new ListSelectionHelper<PropertyName>();
            Properties
                .Select(prop => Vocabulary.getPropertyNames(prop))
                .Subscribe(Values);

            ValidModel
                .CombineLatest(Properties.ItemsObservable, (m, p) => p.FirstOrDefault(prop => prop.PropertyID == m.PropertyID))
                .BindTo(Properties, x => x.SelectedItem);

            ValidModel
                .CombineLatest(Values.ItemsObservable, (m, p) => p.FirstOrDefault(prop => prop.PropertyUri == m.PropertyUri))
                .BindTo(Values, x => x.SelectedItem);

            CanSaveObs().Subscribe(_CanSaveSubject);
        }

        private IObservable<bool> CanSaveObs()
        {            
            var canSave1 = Properties
                .Select(x => x!=null)
                .StartWith(false);


            var canSave2 = Values
                 .Select(x => x != null)
                 .StartWith(false);

            return Extensions.BooleanAnd(canSave1, canSave2);
        }         


        protected override void UpdateModel()
        {           
            Current.Model.PropertyID = Properties.SelectedItem.PropertyID;
            Current.Model.PropertyUri = Values.SelectedItem.PropertyUri;
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
