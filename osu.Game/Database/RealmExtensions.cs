// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Realms;

namespace osu.Game.Database
{
    public static class RealmExtensions
    {
        /// <summary>
        /// Perform a write operation against the provided realm instance.
        /// </summary>
        /// <remarks>
        /// This will automatically start a transaction if not already in one.
        /// </remarks>
        /// <param name="realm">The realm to operate on.</param>
        /// <param name="function">The write operation to run.</param>
        public static void Write(this Realm realm, Action<Realm> function)
        {
            Transaction? transaction = null;

            try
            {
                if (!realm.IsInTransaction)
                    transaction = realm.BeginWrite();

                function(realm);

                transaction?.Commit();
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        /// <summary>
        /// Perform a write operation against the provided realm instance.
        /// </summary>
        /// <remarks>
        /// This will automatically start a transaction if not already in one.
        /// </remarks>
        /// <param name="realm">The realm to operate on.</param>
        /// <param name="function">The write operation to run.</param>
        public static T Write<T>(this Realm realm, Func<Realm, T> function)
        {
            Transaction? transaction = null;

            try
            {
                if (!realm.IsInTransaction)
                    transaction = realm.BeginWrite();

                var result = function(realm);

                transaction?.Commit();

                return result;
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        /// <summary>
        /// Whether the provided change set has changes to the top level collection.
        /// </summary>
        /// <remarks>
        /// Realm subscriptions fire on both collection and property changes (including *all* nested properties).
        /// Quite often we only care about changes at a collection level. This can be used to guard and early-return when no such changes are in a callback.
        /// </remarks>
        public static bool HasCollectionChanges(this ChangeSet changes) => changes.InsertedIndices.Length > 0 || changes.DeletedIndices.Length > 0 || changes.Moves.Length > 0;
    }
}
