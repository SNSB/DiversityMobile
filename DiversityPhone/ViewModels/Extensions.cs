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
using System.Collections.Generic;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public static class Extensions
    {
        public static int ListFindIndex<T>(this IList<T> This, Func<T,bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            for (int idx = 0; idx < This.Count; idx++)
                if (predicate(This[idx]))
                    return idx;

            return -1;
        }

        public static IObservable<bool> BooleanAnd(this IObservable<bool> This, params IObservable<bool>[] parameters)
        {            
            return This.CombineLatestMany((a, b) => a && b, parameters);
        }

        public static IObservable<bool> BooleanOr(this IObservable<bool> This, params IObservable<bool>[] parameters)
        {
            return This.CombineLatestMany((a, b) => a || b, parameters);
        }

        private static IObservable<T> CombineLatestMany<T>(this IObservable<T> This, Func<T, T, T> aggregator, params IObservable<T>[] parameters)
        {
            if (This == null)
                throw new ArgumentNullException("This");
            if (aggregator == null)
                throw new ArgumentNullException("aggregator");

            if (parameters != null)            
            {
                foreach (var parameter in parameters)
                    if (parameter != null)
                        This = This.CombineLatest(parameter, aggregator);
            }
            return This;
        }
    }
}
