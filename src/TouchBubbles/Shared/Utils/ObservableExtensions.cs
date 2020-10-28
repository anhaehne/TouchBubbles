using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace System.Reactive.Linq
{
    public static class ObservableExtensions
    {
        public static IDisposable SubscribeAsync<T>(this IObservable<T> source,
            Func<Task> asyncAction, Action<Exception>? handler = null)
        {
            Func<T, Task<string>> wrapped = async t =>
            {
                await asyncAction();
                return string.Empty;
            };
            if (handler == null)
                return source.SelectMany(wrapped).Subscribe(_ => { });
            return source.SelectMany(wrapped).Subscribe(_ => { }, handler);
        }

        public static IDisposable SubscribeAsync<T>(this IObservable<T> source,
            Func<T, Task> asyncAction, Action<Exception>? handler = null)
        {
            Func<T, Task<string>> wrapped = async t =>
            {
                await asyncAction(t);
                return string.Empty;
            };
            if (handler == null)
                return source.SelectMany(wrapped).Subscribe(_ => { });
            return source.SelectMany(wrapped).Subscribe(_ => { }, handler);
        }

        public static IObservable<IReadOnlyCollection<T>> AsObservable<T>(this ObservableCollection<T> collection) =>
            Observable
                .FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    handler => (sender, args) => handler(args),
                    handler => collection.CollectionChanged += handler,
                    handler => collection.CollectionChanged -= handler)
                .Select(e => collection);
    }
}