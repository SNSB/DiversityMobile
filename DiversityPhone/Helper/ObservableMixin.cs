using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace DiversityPhone.ViewModels
{
    public static class ObservableMixin
    {
        /// <summary>
        /// Specialization of the <see cref="Observable.Catch<T>()"/> Operator that returns an empty observable on error.
        /// </summary>
        public static IObservable<T> CatchEmpty<T>(this IObservable<T> This)
        {
            if (This == null)
            {
                throw new ArgumentNullException("This");
            }

            return This.Catch(Observable.Empty<T>());
        }

        public static IObservable<T> ReturnAndNever<T>(T value)
        {
            return Observable.Return(value).Concat(Observable.Never(value));
        }

        /// <summary>
        /// Samples the Most Recent Value received on <paramref name="This"/> whenever <paramref name="Sampler"/> fires.
        /// Sampling Events are ignored until the first Value is available.
        /// </summary>
        /// <typeparam name="T">Element type of <paramref name="This"/></typeparam>
        /// <typeparam name="TIgnore">Element type of <paramref name="Sampler"/></typeparam>
        /// <param name="This">The Observable whose most recent value will be sampled</param>
        /// <param name="Sampler">The Observable that will provide sampling events</param>
        /// <returns></returns>
        public static IObservable<T> SampleMostRecent<T, TIgnore>(this IObservable<T> This, IObservable<TIgnore> Sampler)
        {
            Contract.Requires(This != null);
            Contract.Requires(Sampler != null);

            var mostrecent = This.MostRecent(default(T)).GetEnumerator();

            return This.Take(1)
                .SelectMany(first =>
                        Sampler
                            .Select(_ =>
                            {
                                mostrecent.MoveNext();
                                return mostrecent.Current;
                            }).Finally(mostrecent.Dispose)
                        );
        }

        public static IDisposable SubscribeCommand<T>(this IObservable<T> This, ICommand Command)
        {
            if (This == null)
            {
                throw new ArgumentNullException("This");
            }
            if (Command == null)
            {
                throw new ArgumentNullException("Command");
            }

            return This
                .Select(t => t as object)
                .Synchronize(Command)
                .Where(Command.CanExecute)
                .Subscribe(Command.Execute);
        }
    }
}
