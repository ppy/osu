// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using Realms;

#nullable enable

namespace osu.Game.Database
{
    /// <summary>
    /// Provides a method of passing realm live objects across threads in a safe fashion.
    /// </summary>
    /// <remarks>
    /// To consume this as a live instance, the live object should be stored and accessed via <see cref="Get"/> each time.
    /// To consume this as a detached instance, assign to a variable of type <see cref="T"/>. The implicit conversion will handle detaching an instance.
    /// </remarks>
    /// <typeparam name="T">The underlying object type. Should be a <see cref="RealmObject"/> with a primary key provided via <see cref="IHasGuidPrimaryKey"/>.</typeparam>
    public class Live<T> : IEquatable<Live<T>>
        where T : RealmObject, IHasGuidPrimaryKey
    {
        /// <summary>
        /// The primary key of the object.
        /// </summary>
        public Guid ID { get; }

        private readonly ThreadLocal<T> threadValues;

        private readonly IRealmFactory contextFactory;

        public Live(T original, IRealmFactory contextFactory)
        {
            this.contextFactory = contextFactory;
            ID = original.Guid;

            var originalContext = original.Realm;

            threadValues = new ThreadLocal<T>(() =>
            {
                var context = this.contextFactory.Get();

                if (context == null || originalContext?.IsSameInstance(context) != false)
                    return original;

                return context.Find<T>(ID);
            });
        }

        /// <summary>
        /// Retrieve a live reference to the data.
        /// </summary>
        public T Get() => threadValues.Value;

        /// <summary>
        /// Wrap a property of this instance as its own live access object.
        /// </summary>
        /// <param name="lookup">The child to return.</param>
        /// <typeparam name="TChild">The underlying child object type. Should be a <see cref="RealmObject"/> with a primary key provided via <see cref="IHasGuidPrimaryKey"/>.</typeparam>
        /// <returns>A wrapped instance of the child.</returns>
        public Live<TChild> WrapChild<TChild>(Func<T, TChild> lookup)
            where TChild : RealmObject, IHasGuidPrimaryKey => new Live<TChild>(lookup(Get()), contextFactory);

        /// <summary>
        /// Perform a write operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public void PerformUpdate(Action<T> perform)
        {
            using (contextFactory.GetForWrite())
                perform(Get());
        }

        public static implicit operator T?(Live<T>? wrapper)
            => wrapper?.Get().Detach();

        public static implicit operator Live<T>(T obj) => obj.WrapAsUnmanaged();

        public bool Equals(Live<T>? other) => other != null && other.ID == ID;

        public override string ToString() => Get().ToString();
    }
}
