using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
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

        public static IObservable<IReadOnlyCollection<T>> ToCollectionObservable<T>(
            this ObservableCollection<T> collection)
        {
            var subject = new BehaviorSubject<IReadOnlyCollection<T>>(collection);
            collection.CollectionChanged += (sender, args) => subject.OnNext(collection);
            return subject;
        }
    }
}