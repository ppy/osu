// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using Realms;

#nullable enable

namespace osu.Game.Database
{
    public class Live<T> : IEquatable<Live<T>>
        where T : RealmObject, IHasGuidPrimaryKey
    {
        public Guid ID { get; }

        private readonly ThreadLocal<T> threadValues;

        public readonly IRealmFactory ContextFactory;

        public Live(T original, IRealmFactory contextFactory)
        {
            ContextFactory = contextFactory;
            ID = original.Guid;

            var originalContext = original.Realm;

            threadValues = new ThreadLocal<T>(() =>
            {
                var context = ContextFactory.Get();

                if (context == null || originalContext?.IsSameInstance(context) != false)
                    return original;

                return context.Find<T>(ID);
            });
        }

        public T Get() => threadValues.Value;

        public Live<TChild> WrapChild<TChild>(Func<T, TChild> lookup)
            where TChild : RealmObject, IHasGuidPrimaryKey => new Live<TChild>(lookup(Get()), ContextFactory);

        public void PerformUpdate(Action<T> perform)
        {
            using (ContextFactory.GetForWrite())
                perform(Get());
        }

        public static implicit operator T?(Live<T>? wrapper)
            => wrapper?.Get().Detach();

        public static implicit operator Live<T>(T obj) => obj.WrapAsUnmanaged();

        public bool Equals(Live<T>? other) => other != null && other.ID == ID;

        public override string ToString() => Get().ToString();
    }
}
