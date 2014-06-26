namespace DiversityPhone {
    using System;
    using System.Diagnostics.Contracts;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Windows.Input;

    public static class ObservableMixin {
        /// <summary>
        /// Specialization of the <see cref="Observable.Catch<T>()"/> Operator that returns an empty observable on error.
        /// </summary>
        public static IObservable<T> CatchEmpty<T>(this IObservable<T> This) {
            if (This == null) {
                throw new ArgumentNullException("This");
            }

            return This.Catch(Observable.Empty<T>());
        }

        public static IObservable<T> ReturnAndNever<T>(T value) {
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
        public static IObservable<T> SampleMostRecent<T, TIgnore>(this IObservable<T> This, IObservable<TIgnore> Sampler) {
            Contract.Requires(This != null);
            Contract.Requires(Sampler != null);

            var mostrecent = This.MostRecent(default(T)).GetEnumerator();

            return This.Take(1)
                .SelectMany(first =>
                        Sampler
                            .Select(_ => {
                                mostrecent.MoveNext();
                                return mostrecent.Current;
                            }).Finally(mostrecent.Dispose)
                        );
        }

        public static IDisposable SubscribeCommand<T>(this IObservable<T> This, ICommand Command) {
            if (This == null) {
                throw new ArgumentNullException("This");
            }
            if (Command == null) {
                throw new ArgumentNullException("Command");
            }

            return This
                .Select(t => t as object)
                .Synchronize(Command)
                .Where(Command.CanExecute)
                .Subscribe(Command.Execute);
        }

        public static IObservable<T1> Fst<T1, T2>(this IObservable<Tuple<T1, T2>> This) {
            Contract.Requires(This != null);

            return This.Select(t => t.Item1);
        }

        public static IObservable<T2> Snd<T1, T2>(this IObservable<Tuple<T1, T2>> This) {
            Contract.Requires(This != null);

            return This.Select(t => t.Item2);
        }

        public static IObservable<T> PermaRef<T>(this IConnectableObservable<T> This)
        {
            Contract.Requires(This != null);

            This.Connect();

            return This;
        }

        public static IObservable<T> StartWithCancellation<T>(Action<CancellationToken, IObserver<T>> task)
        {
            return Observable.Create<T>(obs =>
            {
                var cancelSource = new CancellationTokenSource();
                Observable.Start(() => task(cancelSource.Token, obs))
                    .Subscribe(_2 => { }, obs.OnError, obs.OnCompleted);

                return Disposable.Create(cancelSource.Cancel);
            });
        }
    }
}
