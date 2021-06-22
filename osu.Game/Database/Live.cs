// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Realms;

#nullable enable

namespace osu.Game.Database
{
    /// <summary>
    /// Provides a method of passing realm live objects across threads in a safe fashion.
    /// </summary>
    /// <remarks>
    /// To consume this as a live instance, the live object should be stored and accessed via <see cref="Get"/> each time.
    /// To consume this as a detached instance, assign to a variable of type <typeparam ref="T"/>. The implicit conversion will handle detaching an instance.
    /// </remarks>
    /// <typeparam name="T">The underlying object type. Should be a <see cref="RealmObject"/> with a primary key provided via <see cref="IHasGuidPrimaryKey"/>.</typeparam>
    public class Live<T> : IEquatable<Live<T>>, IHasGuidPrimaryKey, ILiveData
        where T : RealmObject, IHasGuidPrimaryKey
    {
        /// <summary>
        /// The primary key of the object.
        /// </summary>
        public Guid ID { get; set; }

        private readonly List<Action<T>> bindActions = new List<Action<T>>();

        private T current;

        private readonly IRealmFactory? contextFactory;

        private Realm? retrievalContext;

        public Live(T item, IRealmFactory? contextFactory)
        {
            this.contextFactory = contextFactory;

            ID = item.ID;
            current = item;
        }

        private T getThreadLocalValue()
        {
            retrievalContext = contextFactory?.Context;

            // only use the original if no context is available or the source realm is the same.
            if (retrievalContext == null || current.Realm?.IsClosed == false && current.Realm.IsSameInstance(retrievalContext)) return current;

            return retrievalContext.Find<T>(ID);
        }

        /// <summary>
        /// Retrieve a live reference to the data.
        /// </summary>
        public T Get() => current = getThreadLocalValue();

        /// <summary>
        /// Retrieve a detached copy of the data.
        /// </summary>
        public T Detach() => Get().Detach();

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
            using (contextFactory?.GetForWrite())
                perform(Get());
        }

        public static implicit operator T?(Live<T>? wrapper)
            => wrapper?.Detach() ?? null;

        public static implicit operator Live<T>(T obj) => obj.WrapAsUnmanaged();

        public bool Equals(Live<T>? other) => other != null && other.ID == ID;

        public override string ToString() => Get().ToString();

        public void RunBindActions()
        {
            var fetched = Get();

            foreach (var action in bindActions)
                action(fetched);
        }

        public void Bind(Action<T> action)
        {
            bool isFirstAction = bindActions.Count == 0;

            bindActions.Add(action);
            action(Get());

            if (isFirstAction)
                contextFactory?.BindLive(this);
        }
    }
}
