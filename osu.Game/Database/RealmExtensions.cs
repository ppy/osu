// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Realms;

namespace osu.Game.Database
{
    public static class RealmExtensions
    {
        /// <summary>
        /// Performs a <see cref="Realm.Find{T}(System.Nullable{long})"/>.
        /// If a match was not found, a <see cref="Realm.Refresh"/> is performed before trying a second time.
        /// This ensures that an instance is found even if the realm requested against was not in a consistent state.
        /// </summary>
        /// <param name="realm">The realm to operate on.</param>
        /// <param name="id">The ID of the entity to find in the realm.</param>
        /// <typeparam name="T">The type of the entity to find in the realm.</typeparam>
        /// <returns>
        /// The retrieved entity of type <typeparamref name="T"/>.
        /// Can be <see langword="null"/> if the entity is still not found by <paramref name="id"/> even after a refresh.
        /// </returns>
        public static T? FindWithRefresh<T>(this Realm realm, Guid id) where T : IRealmObject
        {
            var found = realm.Find<T>(id);

            if (found == null)
            {
                // It may be that we access this from the update thread before a refresh has taken place.
                // To ensure that behaviour matches what we'd expect (the object generally *should be* available), force
                // a refresh to bring in any off-thread changes immediately.
                realm.Refresh();
                found = realm.Find<T>(id);
            }

            return found;
        }

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
