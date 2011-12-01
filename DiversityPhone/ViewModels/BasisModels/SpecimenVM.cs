namespace DiversityPhone.ViewModels
{
    using System;
    using ReactiveUI;
    using System.Collections.Generic;
    using DiversityPhone.Model;
    using ReactiveUI.Xaml;
    using DiversityPhone.Messages;

    public class SpecimenVM : ReactiveObject
    {
        IList<IDisposable> _subscriptions;

        IMessageBus _messenger;

        public ReactiveCommand Select { get; private set; }
        public ReactiveCommand Edit { get; private set; }

        public Specimen Model { get; private set; }
        public string Description { get { return string.Format("[{0}] {1}", Model.CollectionSpecimenID, Model.AccesionNumber ?? ""); } }
        public Icon Icon { get; private set; }

        public SpecimenVM(IMessageBus messenger, Specimen model)
        {
            if (messenger == null) throw new ArgumentNullException("messenger");
            if (model == null) throw new ArgumentNullException("model");
            Icon = Icon.Specimen;

            _messenger = messenger;
            Model = model;

            _subscriptions = new List<IDisposable>()
            {
                (Select = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<NavigationMessage>(
                        new NavigationMessage( Services.Page.ViewCS, Model.CollectionSpecimenID.ToString())
                        )),
                (Edit = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<NavigationMessage>(
                        new NavigationMessage(Services.Page.EditCS, Model.CollectionSpecimenID.ToString())
                        )),

            };
        }


        public static IList<SpecimenVM> getSingleLevelVMFromModelList(IList<Specimen> source, IMessageBus messenger)
        {
            return getVMListFromModelAndFactory(source,
                spec => new SpecimenVM(messenger, spec));
        }

        private static IList<SpecimenVM> getVMListFromModelAndFactory(IList<Specimen> source, Func<Specimen, SpecimenVM> vmFactory)
        {
            return new VirtualizingReadonlyViewModelList<Specimen, SpecimenVM>(
                        source,
                        vmFactory
                        );
        }
    }
}
