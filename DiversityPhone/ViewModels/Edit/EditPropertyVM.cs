namespace DiversityPhone.ViewModels {
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public class EditPropertyVM : EditPageVMBase<EventProperty> {
        readonly IVocabularyService Vocabulary;
        readonly IFieldDataService Storage;

        private ObservableAsyncMRUCache<int, IObservable<PropertyName>> _PropertyNamesCache;
        private IObservable<Property> _Properties;
        private BehaviorSubject<IEnumerable<int>> _UsedProperties;



        #region Properties


        public bool IsNew { get { return _IsNew.Value; } }
        private ObservableAsPropertyHelper<bool> _IsNew;


        private string _FilterString;

        public string FilterString {
            get {
                return _FilterString;
            }
            set {
                this.RaiseAndSetIfChanged(x => x.FilterString, ref _FilterString, value);
            }
        }


        private Property NoProperty = new Property() { DisplayText = DiversityResources.Setup_Item_PleaseChoose };
        public ListSelectionHelper<Property> Properties { get; private set; }

        private PropertyName NoValue = new PropertyName() { DisplayText = DiversityResources.Setup_Item_PleaseChoose };
        public ListSelectionHelper<PropertyName> Values { get; private set; }
        #endregion


        public EditPropertyVM(
            IVocabularyService Vocabulary,
            IFieldDataService Storage,
            IMessageBus Messenger,
            [Dispatcher] IScheduler Dispatcher
            ) {
            Contract.Requires(Vocabulary != null);
            Contract.Requires(Storage != null);
            this.Vocabulary = Vocabulary;
            this.Storage = Storage;

            _UsedProperties = new BehaviorSubject<IEnumerable<int>>(Enumerable.Empty<int>());
            Messenger.Listen<IEnumerable<int>>(VMMessages.USED_EVENTPROPERTY_IDS)
                .Subscribe(_UsedProperties);


            var properties = Vocabulary.getAllProperties()
                    .ToObservable(ThreadPoolScheduler.Instance)
                    .Replay(ThreadPoolScheduler.Instance);

            this.FirstActivation()
                .Subscribe(_ => properties.Connect());

            _Properties = properties;

            _PropertyNamesCache = new ObservableAsyncMRUCache<int, IObservable<PropertyName>>(
                propertyID =>
                    Observable.Start(
                    () => Vocabulary
                    .getPropertyNames(propertyID)
                    .ToObservable(ThreadPoolScheduler.Instance)
                    .Replay(ThreadPoolScheduler.Instance))
                    .Do(s => s.Connect())
                    .Select(s => s as IObservable<PropertyName>)
                    , 3);

            _IsNew = this.ObservableToProperty(CurrentModelObservable.Select(m => m.IsNew()), x => x.IsNew, false);


            Properties = new ListSelectionHelper<Property>(Dispatcher);
            ModelByVisitObservable
                .SelectMany(evprop => {
                    var isNew = evprop.IsNew();
                    var usedProps = _UsedProperties.First();

                    return
                     _Properties
                         .Where(p =>
                             p.PropertyID == evprop.PropertyID //Editing property -> can't change type
                             || (isNew && !usedProps.Contains(p.PropertyID)))//New Property, only show unused ones                            
                         .ToList();
                })
                .Select(coll => coll as IList<Property>)
                .ObserveOn(Dispatcher)
                .Subscribe(Properties.ItemsObserver);

            Properties.ItemsObservable
                .Where(items => items.Count > 0)
                .Select(items => items[0])
                .Subscribe(i => Properties.SelectedItem = i);

            Values = new ListSelectionHelper<PropertyName>(Dispatcher);
            Properties.SelectedItemObservable
                .SelectMany(prop => {
                    return
                        (prop == null || prop == NoProperty)
                        ? Observable.Return(Observable.Return(NoValue))
                        : _PropertyNamesCache.AsyncGet(prop.PropertyID);
                })
                    .CombineLatest(
                        this.ObservableForProperty(x => x.FilterString).Value()
                        .StartWith(string.Empty)
                        .Select(filter => (filter ?? string.Empty).ToLowerInvariant())
                        .Throttle(TimeSpan.FromMilliseconds(500))
                        .DistinctUntilChanged(),
                        (props, filter) => {
                            var separators = new char[] { ' ', '-' };
                            int max_values = 10;
                            return (from x in props
                                    where x.PropertyUri == Current.Model.PropertyUri  //Always show currently selected value
                                         || (from word in x.DisplayText.ToLowerInvariant().Split(separators) //And all matching ones
                                             select word.StartsWith(filter)).Any(v => v)
                                    select x)
                                    .Take(max_values)
                                    .ToList()
                                    .First();
                        })
                //Reselect value that was selected                    
                    .Select(coll => coll as IList<PropertyName>)
                    .ObserveOn(Dispatcher)
                    .Do(values => Values.SelectedItem =
                        values
                            .Where(item => item.PropertyUri == Current.Model.PropertyUri)
                            .FirstOrDefault()
                        )
                .Subscribe(Values.ItemsObserver);


            CanSaveObs()
                .Subscribe(CanSaveSubject.OnNext);
        }


        private IObservable<bool> CanSaveObs() {
            var propSelected = Properties.SelectedItemObservable
                .Select(x => x != NoProperty && x != null)
                .StartWith(false);

            var valueSelected = Values.SelectedItemObservable
                 .Select(x => x != NoValue && x != null)
                 .StartWith(false);

            return Extensions.BooleanAnd(propSelected, valueSelected);
        }


        protected override void UpdateModel() {
            Current.Model.PropertyID = Properties.SelectedItem.PropertyID;
            Current.Model.PropertyUri = Values.SelectedItem.PropertyUri;
            Current.Model.DisplayText = Values.SelectedItem.DisplayText;
        }
    }
}
