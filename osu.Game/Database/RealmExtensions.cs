// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Realms;

#nullable enable

namespace osu.Game.Database
{
    public static class RealmExtensions
    {
        public static void Write(this Realm realm, Action<Realm> function)
        {
            using var transaction = realm.BeginWrite();
            function(realm);
            transaction.Commit();
        }

        public static T Write<T>(this Realm realm, Func<Realm, T> function)
        {
            using var transaction = realm.BeginWrite();
            var result = function(realm);
            transaction.Commit();
            return result;
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
