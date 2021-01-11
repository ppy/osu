// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Realms;

namespace osu.Game.Database
{
    [SuppressMessage("ReSharper", "CA2225")]
    public class RealmWrapper<T> : IEquatable<RealmWrapper<T>>
        where T : RealmObject, IHasGuidPrimaryKey
    {
        public Guid ID { get; }

        private readonly ThreadLocal<T> threadValues;

        public readonly IRealmFactory ContextFactory;

        public RealmWrapper(T original, IRealmFactory contextFactory)
        {
            ContextFactory = contextFactory;
            ID = original.Guid;

            var originalContext = original.Realm;

            threadValues = new ThreadLocal<T>(() =>
            {
                var context = ContextFactory?.Get();

                if (context == null || originalContext?.IsSameInstance(context) != false)
                    return original;

                return context.Find<T>(ID);
            });
        }

        public T Get() => threadValues.Value;

        public RealmWrapper<TChild> WrapChild<TChild>(Func<T, TChild> lookup)
            where TChild : RealmObject, IHasGuidPrimaryKey => new RealmWrapper<TChild>(lookup(Get()), ContextFactory);

        public void PerformUpdate(Action<T> perform)
        {
            using (ContextFactory.GetForWrite())
                perform(this);
        }

        // ReSharper disable once CA2225
        public static implicit operator T(RealmWrapper<T> wrapper)
            => wrapper?.Get().Detach();

        // ReSharper disable once CA2225
        public static implicit operator RealmWrapper<T>(T obj) => obj.WrapAsUnmanaged();

        public bool Equals(RealmWrapper<T> other) => other != null && other.ID == ID;

        public override string ToString() => Get().ToString();
    }
}
