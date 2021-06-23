// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Realms;

#nullable enable

namespace osu.Game.Database
{
    /// <summary>
    /// Provides a method of working with realm objects over longer application lifetimes.
    /// </summary>
    /// <remarks>
    /// To consume this as a live instance, the live object should be stored and accessed via <see cref="Get"/> each time.
    /// To consume this as a detached instance, assign to a variable of type <typeparam ref="T"/>. The implicit conversion will handle detaching an instance (and copy all content out).
    /// </remarks>
    /// <typeparam name="T">The underlying object type. Should be a <see cref="RealmObject"/> with a primary key provided via <see cref="IHasGuidPrimaryKey"/>.</typeparam>
    public class Live<T> : IEquatable<Live<T>>, ILiveData
        where T : RealmObject, IHasGuidPrimaryKey
    {
        /// <summary>
        /// The primary key of the object.
        /// </summary>
        public readonly Guid ID;

        private readonly List<Action<T>> bindActions = new List<Action<T>>();

        /// <summary>
        /// The currently retrieved instance of the realm object, sourced from <see cref="retrievalContext"/>.
        /// </summary>
        private T data;

        private readonly IRealmFactory? contextFactory;

        private Realm? retrievalContext;

        /// <summary>
        /// Construct a new instance of live realm data.
        /// </summary>
        /// <param name="data">The data to be consumed.</param>
        /// <param name="contextFactory">A context factory to allow transfer and re-retrieval over thread contexts. May be null if the provided item is not managed.</param>
        public Live(T data, IRealmFactory? contextFactory)
        {
            this.contextFactory = contextFactory;

            if (contextFactory != null && data.Realm == null)
                throw new ArgumentException(@"Provided data object should be fetched from a instance", nameof(data));

            ID = data.ID;
            this.data = data;
        }

        /// <summary>
        /// Access the underlying data directly from realm.
        /// </summary>
        public T Get()
        {
            retrievalContext = contextFactory?.Context;

            // unmanaged data.
            if (retrievalContext == null)
                return data;

            // if the retrieved context is still valid, no re-retrieval is required.
            if (!data.Realm.IsClosed && data.Realm.IsSameInstance(retrievalContext))
                return data;

            return data = retrievalContext.Find<T>(ID);
        }

        /// <summary>
        /// Retrieve a detached copy of the data.
        /// </summary>
        public T Detach() => Get().Detach();

        /// <summary>
        /// Perform a write operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public void PerformUpdate(Action<T> perform)
        {
            using (contextFactory?.GetForWrite())
                perform(Get());
        }

        /// <summary>
        /// Re-run bind actions on the current context.
        /// Should only be called after a context switch occurs.
        /// </summary>
        public void RunBindActions()
        {
            var previousContext = retrievalContext;
            var fetched = Get();
            Debug.Assert(!ReferenceEquals(previousContext, retrievalContext));

            foreach (var action in bindActions)
                action(fetched);
        }

        /// <summary>
        /// Initialise any property / value change bindings.
        /// </summary>
        /// <remarks>
        /// These will automatically be re-bound even if the original context is lost due to a thread change.
        /// </remarks>
        /// <param name="action">The binding actions to perform.</param>
        public void Bind(Action<T> action)
        {
            bool isFirstAction = bindActions.Count == 0;

            bindActions.Add(action);
            action(Get());

            if (isFirstAction)
                contextFactory?.BindLive(this);
        }

        /// <summary>
        /// Wrap a property of this instance as its own live access object.
        /// </summary>
        /// <param name="lookup">The child to return.</param>
        /// <typeparam name="TChild">The underlying child object type. Should be a <see cref="RealmObject"/> with a primary key provided via <see cref="IHasGuidPrimaryKey"/>.</typeparam>
        /// <returns>A wrapped instance of the child.</returns>
        public Live<TChild> WrapChild<TChild>(Func<T, TChild> lookup)
            where TChild : RealmObject, IHasGuidPrimaryKey => new Live<TChild>(lookup(Get()), contextFactory);

        public static implicit operator T?(Live<T>? wrapper)
            => wrapper?.Detach() ?? null;

        public static implicit operator Live<T>(T obj) => obj.WrapAsUnmanaged();

        public bool Equals(Live<T>? other) => other != null && other.ID == ID;

        public override string ToString() => Get().ToString();
    }
}
