// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Realms;

#nullable enable

namespace osu.Game.Database
{
    public interface IRealmFactory
    {
        /// <summary>
        /// The main realm context, bound to the update thread.
        /// </summary>
        Realm Context { get; }

        /// <summary>
        /// Get a fresh context for read usage.
        /// </summary>
        RealmContextFactory.RealmUsage GetForRead();

        /// <summary>
        /// Request a context for write usage.
        /// This method may block if a write is already active on a different thread.
        /// </summary>
        /// <returns>A usage containing a usable context.</returns>
        RealmContextFactory.RealmWriteUsage GetForWrite();

        /// <summary>
        /// Create an instance of live realm data, allowing bindings and subscriptions.
        /// Must be run from the update thread.
        /// </summary>
        /// <param name="query">The query to construct (and on context loss, reconstruct) the value.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>A live instance.</returns>
        Live<T> CreateLive<T>(Func<Realm, T> query) where T : class;
    }
}
