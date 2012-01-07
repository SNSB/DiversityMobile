using System;
using ReactiveUI;
using System.Reactive.Linq;
using System.Collections.Generic;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using System.Reactive.Subjects;
using System.Linq;

namespace DiversityPhone.ViewModels
{
  

    public class ViewIUVM : PageViewModel
    {
        public enum Pivots
        {
            Subunits,
            Analyses,
            Descriptions,
            Multimedia
        }        

        #region Services        
        IOfflineStorage _storage;
        #endregion

        #region Commands
        public ReactiveCommand Add { get; private set; }
        #endregion

        #region Properties

        private Pivots _SelectedPivot;
        public Pivots SelectedPivot
        {
            get
            {
                return _SelectedPivot;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SelectedPivot, ref _SelectedPivot, value);
            }
        }

        private ObservableAsPropertyHelper<IdentificationUnitVM> _Current;
        public IdentificationUnitVM Current { get { return _Current.Value; } }             

        private ObservableAsPropertyHelper<IList<IdentificationUnitVM>> _Subunits;
        public IList<IdentificationUnitVM> Subunits { get { return _Subunits.Value; } }
        
        #endregion

        

        public ViewIUVM(IMessageBus messenger, IOfflineStorage storage)
            : base(messenger)
        {
            
            _storage = storage;     
       
            var rawModel = 
                StateObservable
                .Select(s=>UnitFromContext(s.Context));

            var unitDeletedMessageSource =
                rawModel
                .Where(iu=> iu == null)
                .Select(_ => Message.NavigateBack);
            Messenger.RegisterMessageSource(unitDeletedMessageSource);

            var validModel =
                rawModel
                .Where(iu => iu != null);        
            

            _Current = validModel
                .Select(iu => new IdentificationUnitVM(Messenger, iu, null))
                .ToProperty(this, x => x.Current);
            _Subunits = validModel
                .Select(iu => getSubUnits(iu))
                .ToProperty(this, vm => vm.Subunits);

            Add = new ReactiveCommand();
            var addMessageSource = 
                Add
                .Select(_ =>
                    {
                        switch(SelectedPivot)
                        {
                            case Pivots.Analyses:
                                return Page.EditIUAN;
                            case Pivots.Multimedia:
                                //TODO Multimedia Page
                            case Pivots.Descriptions:
                                //TODO Description Page
                            case Pivots.Subunits:
                                return Page.EditIU;
                            default:
                                return Page.EditIU;
                        }
                    })
                .Select(p => new NavigationMessage(p,null, ReferrerType.IdentificationUnit, Current.Model.UnitID.ToString()));
            Messenger.RegisterMessageSource(addMessageSource);         
        }

        private IdentificationUnit UnitFromContext(string ctx)
        {
            if (ctx != null)
            {
                int id;
                if (int.TryParse(ctx, out id))
                {
                    return _storage.getIdentificationUnitByID(id);
                }               
            }
            return null;
        } 
      
        private IList<IdentificationUnitVM> getSubUnits(IdentificationUnit iu)
        {
            return IdentificationUnitVM.getTwoLevelVMFromModelList(_storage.getSubUnits(iu),
                iu2 => _storage.getSubUnits(iu2),
                Messenger);                
        }
    }
}
