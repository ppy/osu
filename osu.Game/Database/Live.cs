// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Development;
using Realms;

#nullable enable

namespace osu.Game.Database
{
    /// <summary>
    /// Provides a method of working with realm objects over longer application lifetimes.
    /// </summary>
    /// <typeparam name="T">The underlying object type.</typeparam>
    public class Live<T> : IRealmBindableActions
        where T : class
    {
        private readonly Func<Realm, T> query;

        private readonly IRealmFactory realm;

        /// <summary>
        /// The currently retrieved instance of the realm data, sourced from the <see cref="retrievedContext"/>.
        /// </summary>
        private T retrievedValue;

        private Realm? retrievedContext;

        /// <summary>
        /// Construct a new instance of live realm data.
        /// </summary>
        /// <param name="query">A function which initialises and returns the value.</param>
        /// <param name="realm">A context factory to allow transfer and re-retrieval over thread contexts.</param>
        public Live(Func<Realm, T> query, IRealmFactory realm)
        {
            this.query = query;
            this.realm = realm;

            retrievedValue = fetchThreadLocalValue();
        }

        /// <summary>
        /// Access the underlying value directly from realm.
        /// </summary>
        public T Value => fetchThreadLocalValue();

        /// <summary>
        /// Perform a write operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public void PerformUpdate(Action<T> perform)
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            using (var usage = realm.GetForWrite())
            {
                // we are on the update thread so the context should always match our retrieved context.
                // if this isn't the case things are going to fall over when making changes.
                Debug.Assert(ReferenceEquals(usage.Realm, retrievedContext));

                perform(retrievedValue);

                usage.Commit();
            }
        }

        /// <summary>
        /// Fetches the value represented by this instance on the current update thread context.
        /// </summary>
        /// <returns></returns>
        private T fetchThreadLocalValue()
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            var localThreadContext = realm.Context;

            // if the retrieved context is still valid, no re-retrieval is required.
            if (retrievedContext?.IsClosed == false && retrievedContext.IsSameInstance(localThreadContext))
                return retrievedValue;

            retrievedContext = localThreadContext;
            return retrievedValue = query(retrievedContext);
        }

        public override string ToString() => Value.ToString();

        void IRealmBindableActions.RunSetupAction() => fetchThreadLocalValue();
    }
}
