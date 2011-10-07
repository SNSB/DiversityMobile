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
using System.Collections.Generic;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{
    public class SpecimenVM:ReactiveObject
    {
         IMessageBus _messenger;        
        IList<IDisposable> _subscriptions;

        public Specimen Model { get; private set; }
        public string Description { get { return string.Format("[{0}] {1}", Model.CollectionSpecimenID, Model.AccesionNumber ?? ""); } }

        public ReactiveCommand Select { get; private set; }

        public ReactiveCommand Edit { get; private set; }


        public SpecimenVM(IMessageBus messenger, Specimen model)
        {
            if (messenger == null) throw new ArgumentNullException("messenger");
            if (model == null) throw new ArgumentNullException("model");
            

            _messenger = messenger;
            Model = model;

            _subscriptions = new List<IDisposable>()
            {
                (Select = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<Specimen>(Model, MessageContracts.SELECT)),
                (Edit = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<Specimen>(Model, MessageContracts.EDIT)),

            };
        }


        public static IList<SpecimenVM> getSingleLevelVMFromModelList(IList<Specimen> source, IMessageBus messenger)
        {
            return getVMListFromModelAndFactory(source,
                spec => new SpecimenVM(messenger, spec));
        }

        private static IList<SpecimenVM> getVMListFromModelAndFactory(IList<Specimen> source, Func<Specimen,SpecimenVM> vmFactory)
        {
            return new VirtualizingReadonlyViewModelList<Specimen, SpecimenVM>(
                        source,
                        vmFactory
                        );
        }
    }
}
