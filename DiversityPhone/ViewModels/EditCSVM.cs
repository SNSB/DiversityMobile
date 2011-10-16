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
    using DiversityPhone.Model;
    using ReactiveUI.Xaml;
    using DiversityPhone.Messages;
    using System.Collections.Generic;
    using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class EditCSVM:ReactiveObject
    {
        private IList<IDisposable> _subscriptions;

        #region Services
        private IMessageBus _messenger;
        private INavigationService _navigation;
        #endregion

        #region Commands
        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand Cancel { get; private set; }
        #endregion

        #region Properties
        private Specimen _Model;
        public Specimen Model
        {
            get { return _Model; }
            set { this.RaiseAndSetIfChanged(x => x.Model, ref _Model, value); }
        }

        private string _AccessionNumber;
        public string AccessionNumber
        {
            get { return _AccessionNumber; }
            set { this.RaiseAndSetIfChanged(x => x.AccessionNumber, ref _AccessionNumber, value); }
        }

        private int _EventID;
        public int EventID
        {
            get { return _EventID; }
            set { this.RaiseAndSetIfChanged(x => x._EventID, ref _EventID, value); }
        }

        private int _SpecimenID;
        public int SpecimenID
        {
            get { return _SpecimenID; }
            set { this.RaiseAndSetIfChanged(x => x._SpecimenID, ref _SpecimenID, value); }
        }

        public string AccessionDate
        {
            get
            {
                return (Model != null) ? Model.AccessionDate.ToShortDateString() : "";
            }
        }

       
        #endregion


        public EditCSVM(INavigationService nav, IMessageBus messenger)
        {

            _messenger = messenger;
            _navigation = nav;
            _messenger.Listen<Specimen>(MessageContracts.EDIT)
                .Subscribe(cs => updateView(cs));

            var descriptionObservable = this.ObservableForProperty(x => x.AccessionNumber);
            var canSave = descriptionObservable.Select(desc => !string.IsNullOrWhiteSpace(desc.Value)).StartWith(false);

            _subscriptions = new List<IDisposable>()
            {
                (Save = new ReactiveCommand(canSave))               
                    .Subscribe(_ => executeSave()),

                (Cancel = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<Message>(Message.NavigateBack))
            };
        }



        private void executeSave()
        {
            updateModel();
            _messenger.SendMessage<Specimen>(Model, MessageContracts.SAVE);
            _navigation.NavigateBack();
        }

        private void updateModel()
        {
            Model.AccesionNumber = AccessionNumber;
            Model.CollectionEventID = EventID;
            Model.AccessionDate = DateTime.Parse(AccessionDate);
            Model.CollectionSpecimenID = SpecimenID;
        }

        private void updateView(Specimen cs)
        {
            Model = cs;
            AccessionNumber = Model.Description ?? "";
            SeriesCode = Model.SeriesCode;
            SeriesEnd = Model.SeriesEnd;
            this.RaisePropertyChanged(x => x.AccessionDate);
        }



     

    }
}
