using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Input;

namespace DiversityPhone.ViewModels
{
    public static class RelayCommandMixin
    {
        public static ICommand Relay<T>(this ICommand This, Predicate<T> canExecute = null, Func<object, T> mapParameter = null, IObservable<Unit> canExecuteChanged = null)
            where T : class
        {
            return new RelayCommand<T>(This, canExecute, mapParameter, canExecuteChanged);
        }
    }

    internal class RelayCommand<T> : ICommand, IDisposable where T : class
    {
        private readonly ICommand Inner;
        private readonly Func<object, T> MapParameter;
        private readonly Predicate<T> CanExecute;
        private readonly IDisposable Subscription;

        public RelayCommand(ICommand inner, Predicate<T> canExecute = null, Func<object, T> mapParameter = null, IObservable<Unit> canExecuteChanged = null)
        {
            Inner = inner;
            MapParameter = mapParameter ?? ((x) => x as T);
            CanExecute = canExecute ?? ((x) => x != null);

            canExecuteChanged = canExecuteChanged ?? Observable.FromEvent<EventHandler, EventArgs>(x => inner.CanExecuteChanged += x, x => inner.CanExecuteChanged -= x).Select(x => Unit.Default);
            Subscription = canExecuteChanged
                .Select(_ => CanExecuteChanged)
                .Where(x => x != null)
                .Subscribe(list => list(this, EventArgs.Empty));
        }

        bool ICommand.CanExecute(object parameter)
        {
            var mapped = MapParameter(parameter);
            return CanExecute(mapped) && Inner.CanExecute(mapped);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var mapped = MapParameter(parameter);
            Inner.Execute(mapped);
        }

        public void Dispose()
        {
            Subscription.Dispose();
        }
    }
}