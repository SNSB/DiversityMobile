using ReactiveUI.Xaml;
using ReactiveUI;
using System.Reactive.Linq;
using System;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using DiversityPhone.Model;
using System.Reactive.Subjects;

namespace DiversityPhone.ViewModels
{
    public abstract class EditElementPageVMBase<T> : ElementPageViewModel<T>, IEditPageVM where T : IModifyable
    {
        private ISubject<DialogResult> DeleteSubject = new Subject<DialogResult>();

        #region Commands
        public IReactiveCommand Save { get; private set; }
        public IReactiveCommand ToggleEditable { get; private set; }
        public IReactiveCommand Delete { get; private set; }
        #endregion

        private ObservableAsPropertyHelper<bool> _IsEditable;
        /// <summary>
        /// Shows, whether the current Object can be Edited
        /// </summary>
        public bool IsEditable
        {
            get
            {
                return _IsEditable.Value;
            }
        }

        /// <summary>
        /// Subject used for the canSave values by default.
        /// </summary>
        protected ISubject<bool> _CanSaveSubject { get; private set; }  

        /// <summary>
        /// Determines, whether Save can execute
        /// </summary>
        /// <returns>Observable that will be used to enable/disable Save</returns>
        protected virtual IObservable<bool> CanSave() 
        {
            return Observable.Return(true);
        }

        /// <summary>
        /// Updates the Model object with any unsaved changes.
        /// </summary>
        protected abstract void UpdateModel();

        /// <summary>
        /// Called Before Saving, but after UpdateModel
        /// </summary>
        protected virtual void OnSave() { }

        /// <summary>
        /// Called Before Deleting
        /// </summary>
        protected virtual void OnDelete() { }

        public EditElementPageVMBase()
            : this(true) { }


        public EditElementPageVMBase(bool refreshModel)
            : base(refreshModel)
        {
            _CanSaveSubject = new Subject<bool>();
            Save = new ReactiveCommand(
                _CanSaveSubject
                );
                        
            Observable.Concat(
                Observable.Return(false),
                CanSave()
                //Observable.Never<bool>()
            )            
            .ObserveOnDispatcher() // Work around bug in ReactiveUI
            .Subscribe(_CanSaveSubject.OnNext);

            Delete = new ReactiveCommand(
                ValidModel
                .Select(m => !m.IsNew())
                );

            ToggleEditable = new ReactiveCommand(
                ValidModel
                .Select(m => !m.IsUnmodified()) //Can't edit uploaded Items               
                );

            _IsEditable = this.ObservableToProperty(
            DistinctStateObservable
               .Select(s => s.Context == null) //Newly created Units are immediately editable
               .Merge(
                   ToggleEditable.Select(_ => !IsEditable) //Toggle Editable
               ),
               vm => vm.IsEditable);

            Messenger.RegisterMessageSource(
               Save
               .Where(_ => Current != null)
               .Do(_ => UpdateModel())
               .Do(_ => OnSave())
               .Select(_ => Current as IElementVM<T>),
               MessageContracts.SAVE);

            Messenger.RegisterMessageSource(
               Delete
               .Select(_ => new DialogMessage(DialogType.YesNo,"", DiversityResources.Message_ConfirmDelete, DeleteSubject.OnNext))
               );

            Messenger.RegisterMessageSource(
                DeleteSubject                
                .Where(result => result == DialogResult.OKYes && Current != null)
                .Select(_ => Current as IElementVM<T>)               
                .Do(_ => OnDelete()),
                MessageContracts.DELETE
                );

            //On Delete or Save, Navigate Back
            Messenger.RegisterMessageSource(
               Save
               .Merge(Delete)
               .Select(_ => Page.Previous)
               );
        }

      
    }
}
