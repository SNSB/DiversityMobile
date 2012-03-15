using ReactiveUI.Xaml;
using ReactiveUI;
using System.Reactive.Linq;
using System;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using DiversityPhone.Model;

namespace DiversityPhone.ViewModels
{
    public abstract class EditElementPageVMBase<T> : ElementPageViewModel<T> where T : IModifyable
    {
        #region Commands
        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand ToggleEditable { get; private set; }
        public ReactiveCommand Delete { get; private set; }
        #endregion

        public ObservableAsPropertyHelper<bool> _IsEditable;
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
        /// Determines, whether Save can execute
        /// </summary>
        /// <returns>Observable that will be used to enable/disable Save</returns>
        protected abstract IObservable<bool> CanSave();

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
            Save = new ReactiveCommand(CanSave());
            Delete = new ReactiveCommand(
                ValidModel
                .Select(m => !m.IsNew())
                );

            ToggleEditable = new ReactiveCommand(
                ValidModel
                .Select(m => !m.IsUnmodified()) //Can't edit uploaded Items               
                );

            _IsEditable = DistinctStateObservable
               .Select(s => s.Context == null) //Newly created Units are immediately editable
               .Merge(
                   ToggleEditable.Select(_ => !IsEditable) //Toggle Editable
               )
               .ToProperty(this, vm => vm.IsEditable);

            Messenger.RegisterMessageSource(
               Save
               .Where(_ => Current != null)
               .Do(_ => UpdateModel())
               .Do(_ => OnSave())
               .Select(_ => Current.Model),
               MessageContracts.SAVE);

            Messenger.RegisterMessageSource(
               Delete
               .Where(_ => Current != null)
               .Select(_ => Current.Model)               
               .Do(_ => OnDelete()),
               MessageContracts.DELETE);

            //On Delete or Save, Navigate Back
            Messenger.RegisterMessageSource(
               Save
               .Merge(Delete)
               .Select(_ => Page.Previous)
               );
        }
    }
}
