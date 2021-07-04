// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Realms;

namespace osu.Game.Database
{
    public interface IRealmFactory
    {
        /// <summary>
        /// The main realm context, bound to the update thread.
        /// If querying from a non-update thread is needed, use <see cref="GetForRead"/> or <see cref="GetForWrite"/> to receive a context instead.
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
    }
}
