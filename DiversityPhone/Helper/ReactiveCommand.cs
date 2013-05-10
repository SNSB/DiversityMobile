﻿using ReactiveUI.Xaml;
using System.Windows.Input;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public class ReactiveCommand<T> : ObservableBase<T>, ICommand where T : class
    {
        private ReactiveCommand inner_command;
        private IObservable<T> inner_observable;

        private static Func<object, bool> create_can_execute<U>(Func<U, bool> canExecute) where U : class
        {
            if (canExecute == null)
                return null;
            else
            {
                return (arg) =>
                {
                    if (arg is U)
                        return canExecute(arg as U);
                    else 
                        return false;
                };
            }
        }

        public ReactiveCommand(Func<T, bool> canExecute = null, IObservable<Unit> canExecuteChanged = null)
        {
            inner_command = ReactiveCommand.Create(create_can_execute(canExecute));
            if (canExecuteChanged != null)
                canExecuteChanged.Subscribe(_ => RaiseCanExecuteChanged(this, EventArgs.Empty));
            CommonConstructor();
        }
        public ReactiveCommand(IObservable<bool> canExecute)
        {
            inner_command = new ReactiveCommand(canExecute);
            inner_command.CanExecuteChanged += RaiseCanExecuteChanged;
            CommonConstructor();
        }

        void RaiseCanExecuteChanged(object sender, EventArgs args)
        {
            var subscribers = CanExecuteChanged;
            if (subscribers != null)
                subscribers(sender, args);
        }

        private void CommonConstructor()
        {
            var obs = inner_command
                .CastNotNull<T>()
                .Publish();
            inner_observable = obs;

            obs.Connect();
        }

        public bool CanExecute(object parameter)
        {            
            return inner_command.CanExecute(parameter as T);
        }

        public event System.EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            inner_command.Execute(parameter as T);
        }

        protected override IDisposable SubscribeCore(IObserver<T> observer)
        {
            return inner_observable.Subscribe(observer);
        }
    }
}
