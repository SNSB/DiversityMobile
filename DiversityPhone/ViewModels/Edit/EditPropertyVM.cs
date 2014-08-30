namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    public class EditPropertyVM : EditPageVMBase<EventProperty>
    {
        private readonly IVocabularyService Vocabulary;
        private readonly IFieldDataService Storage;

        private ObservableAsyncMRUCache<int, IObservable<PropertyName>> _PropertyNamesCache;

        #region Properties

        public bool IsNew { get { return _IsNew.Value; } }

        private ObservableAsPropertyHelper<bool> _IsNew;

        private string _FilterString;

        public string FilterString
        {
            get
            {
                return _FilterString;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.FilterString, ref _FilterString, value);
            }
        }

        private readonly Property NoProperty = new Property() { DisplayText = DiversityResources.Setup_Item_PleaseChoose };
        private readonly IList<Property> DefaultProperties;

        public ListSelectionHelper<Property> Properties { get; private set; }

        private PropertyName NoValue = new PropertyName() { DisplayText = DiversityResources.Setup_Item_PleaseChoose };

        public ListSelectionHelper<PropertyName> Values { get; private set; }

        #endregion Properties

        public EditPropertyVM(
            IVocabularyService Vocabulary,
            IFieldDataService Storage,
            [Dispatcher] IScheduler Dispatcher,
            [ThreadPool] IScheduler ThreadPool
            )
        {
            Contract.Requires(Vocabulary != null);
            Contract.Requires(Storage != null);
            this.Vocabulary = Vocabulary;
            this.Storage = Storage;

            DefaultProperties = new List<Property>() { NoProperty };

            var properties = Messenger.Listen<EventMessage>(MessageContracts.INIT)
                .ObserveOn(ThreadPool)
                .Select(_ => Vocabulary.getAllProperties().ToList() as IList<Property>
                ).Publish();
            properties.Connect();

            // Broadcast latest Properties for other VMs to use
            Messenger.RegisterMessageSource(properties);

            properties
                .Select(props => new ObservableAsyncMRUCache<int, IObservable<PropertyName>>(
                        propertyID =>
                            Observable.Start(
                            () => Vocabulary
                            .getPropertyNames(propertyID)
                            .ToObservable(ThreadPool)
                            .Replay(ThreadPool))
                            .Do(s => s.Connect())
                            .Select(s => s as IObservable<PropertyName>),
                            10
                    ))
                .Subscribe(cache => _PropertyNamesCache = cache);

            _IsNew = this.ObservableToProperty(ModelByVisitObservable.Select(m => m.IsNew()), x => x.IsNew, false);

            Properties = new ListSelectionHelper<Property>(Dispatcher);

            properties.SampleMostRecent(this.OnActivation())
                .Zip(ModelByVisitObservable, (props, evprop) =>
                {
                    var isNew = evprop.IsNew();
                    if (isNew)
                    { //New Property, only show unused ones
                        var usedPropertyIDs = (from p in Storage.getPropertiesForEvent(evprop.EventID)
                                               select p.PropertyID).ToList();
                        return from p in props
                               where !usedPropertyIDs.Contains(p.PropertyID)
                               select p;
                    }
                    else
                    { //Editing property -> can't change type
                        return from p in props
                               where p.PropertyID == evprop.PropertyID
                               select p;
                    }
                })
                .Select(coll => coll.ToList() as IList<Property>)
                .Do(list =>
                {
                    if (list.Count > 1)
                    {
                        list.Insert(0, NoProperty);
                    }
                })
                .ObserveOn(Dispatcher)
                .Subscribe(Properties.ItemsObserver);

            Properties.ItemsObservable
                .Where(items => items.Count > 0)
                .Select(items => items[0])
                .Subscribe(i => Properties.SelectedItem = i);

            Values = new ListSelectionHelper<PropertyName>(Dispatcher);
            Properties.SelectedItemObservable
                .SelectMany(prop =>
                {
                    return
                        (prop == null || prop == NoProperty || _PropertyNamesCache == null)
                        ? Observable.Return(Observable.Return(NoValue))
                        : _PropertyNamesCache.AsyncGet(prop.PropertyID);
                })
                    .CombineLatest(
                        this.ObservableForProperty(x => x.FilterString).Value()
                        .StartWith(string.Empty)
                        .Select(filter => (filter ?? string.Empty).ToLowerInvariant())
                        .Throttle(TimeSpan.FromMilliseconds(500))
                        .DistinctUntilChanged(),
                        (props, filter) =>
                        {
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

        private IObservable<bool> CanSaveObs()
        {
            var propSelected = Properties.SelectedItemObservable
                .Select(x => x != NoProperty && x != null)
                .StartWith(false);

            var valueSelected = Values.SelectedItemObservable
                 .Select(x => x != NoValue && x != null)
                 .StartWith(false);

            return Extensions.BooleanAnd(propSelected, valueSelected);
        }

        protected override async Task UpdateModel()
        {
            Current.Model.PropertyID = Properties.SelectedItem.PropertyID;
            Current.Model.PropertyUri = Values.SelectedItem.PropertyUri;
            Current.Model.DisplayText = Values.SelectedItem.DisplayText;
        }
    }
}