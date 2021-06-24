// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
    public class Live<T> : IRealmBindableActions
        where T : class
    {
        /// <summary>
        /// The currently retrieved instance of the realm data, sourced from <see cref="retrievalContext"/>.
        /// </summary>
        private T data;

        private readonly Func<Realm, T> setup;

        private readonly IRealmFactory? realm;

        private Realm? retrievalContext;

        /// <summary>
        /// Construct a new instance of live realm data.
        /// </summary>
        /// <param name="setup">The data to be consumed.</param>
        /// <param name="realm">A context factory to allow transfer and re-retrieval over thread contexts. May be null if the provided item is not managed.</param>
        public Live(Func<Realm, T> setup, IRealmFactory? realm)
        {
            this.setup = setup;
            this.realm = realm;

            data = Get();
        }

        /// <summary>
        /// Access the underlying data directly from realm.
        /// </summary>
        public T Get()
        {
            var previousContext = retrievalContext;

            retrievalContext = realm?.Context;

            // unmanaged data.
            if (retrievalContext == null)
                return data;

            // if the retrieved context is still valid, no re-retrieval is required.
            if (previousContext?.IsClosed == false && previousContext.IsSameInstance(retrievalContext))
                return data;

            return data = setup(retrievalContext);
        }

        /// <summary>
        /// Retrieve a detached copy of the data.
        /// </summary>
        public T Detach() => Get().Detach(); // TODO: this doesn't work on collections unfortunately..

        /// <summary>
        /// Perform a write operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public void PerformUpdate(Action<T> perform)
        {
            using (realm?.GetForWrite())
                perform(Get());
        }

        public static implicit operator T?(Live<T>? wrapper) => wrapper?.Detach() ?? null;

        public override string ToString() => Get().ToString();

        void IRealmBindableActions.RunSetupAction() => Get();
    }
}
