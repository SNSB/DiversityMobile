using ReactiveUI;
using System.Reactive.Linq;
using ReactiveUI.Xaml;
using DiversityPhone.Model;
using System.Reactive.Subjects;
using DiversityPhone.Services;
using System.Reactive;
using System;

namespace DiversityPhone.ViewModels
{
    public abstract class EditPageVMBase<T> : ElementPageVMBase<T>, IEditPageVM where T : IModifyable, IReactiveNotifyPropertyChanged
    {
        public IReactiveCommand Save { get; private set; }
        public IReactiveCommand ToggleEditable { get; private set; }
        public IReactiveCommand Delete { get; private set; }


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

        protected ISubject<bool> CanSaveSubject { get; private set; }
        private ISubject<Unit> DeleteSubject = new Subject<Unit>();

        public EditPageVMBase(Predicate<T> filter = null)
        {
            CanSaveSubject = new Subject<bool>();
            Save = new ReactiveCommand(CanSaveSubject);
            Save
                .Do(_ => UpdateModel())
                .Select(_ => Current)
                .ToMessage(MessageContracts.SAVE);

            ToggleEditable = new ReactiveCommand(ModelByVisitObservable.Select(m=> !m.IsUnmodified()));
            _IsEditable = this.ObservableToProperty(
                    Observable.Merge(
                        ModelByVisitObservable
                        .Select(m => m.IsNew()),
                        ToggleEditable.Select(_ => !IsEditable)
                    ),
                x => x.IsEditable);

            Delete = new ReactiveCommand(ModelByVisitObservable.Select(m => !m.IsNew()));            
            Delete
                .Select(_ => new DialogMessage(DialogType.YesNo, "", DiversityResources.Message_ConfirmDelete,(res) => {if(res == DialogResult.OKYes) DeleteSubject.OnNext(Unit.Default);}))
                .ToMessage();
               
            DeleteSubject               
                .Select(_ => Current)
                .ToMessage(MessageContracts.DELETE);

            Observable.Merge(Save.Select(_ => Unit.Default), DeleteSubject)
                .Select(_ => Page.Previous)
                .ToMessage();

            Messenger.Listen<IElementVM<T>>(MessageContracts.EDIT)
                .Where(vm => vm != null)
                .Where(vm => filter == null || filter(vm.Model))
                .Subscribe(x => Current = x);
        }

        protected virtual void UpdateModel() {}
    }
}
