using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.AspNetCore.Components
{
    public abstract class RxComponentBase : ComponentBase, IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public new void StateHasChanged()
        {
            base.StateHasChanged();
        }

        public void RegisterDisposable(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) 
                return;

            foreach (var disposable in _disposables)
                disposable.Dispose();
        }
    }

    public static class RxComponentBaseExtensions
    {
        public static void UpdatePropertyWith<T, TThis>(this TThis component, IObservable<T> obs,
            Expression<Func<TThis, T>> propertyAccess)
            where TThis : RxComponentBase
        {
            var member = GetPropertyInfo(propertyAccess);
            var subscription = obs.Subscribe(next =>
            {
                switch (member)
                {
                    case PropertyInfo property:
                        property.SetValue(component, next);
                        break;

                    case FieldInfo field:
                        field.SetValue(component, next);
                        break;

                    default:
                        throw new ArgumentException("Expression must be property or field.", nameof(propertyAccess));
                }

                component.StateHasChanged();
            });
            component.RegisterDisposable(subscription);
        }

        private static MemberInfo GetPropertyInfo<T, TThis>(Expression<Func<TThis, T>> propertyAccess)
        {
            var member = propertyAccess.Body as MemberExpression ??
                         throw new ArgumentException("Expression must be member access.", nameof(propertyAccess));

            return member.Member;
        }
    }
}