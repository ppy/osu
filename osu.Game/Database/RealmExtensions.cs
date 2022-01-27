// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Realms;

namespace osu.Game.Database
{
    public static class RealmExtensions
    {
        public static void Write(this Realm realm, Action<Realm> function)
        {
            // if a higher-level transaction is already taking place, simply proceed with the write.
            // the assumption is that the write will be committed by the aforementioned higher-level transaction.
            if (realm.IsInTransaction)
            {
                function(realm);
                return;
            }

            using var transaction = realm.BeginWrite();
            function(realm);
            transaction.Commit();
        }

        public static T Write<T>(this Realm realm, Func<Realm, T> function)
        {
            // if a higher-level transaction is already taking place, simply proceed with the write.
            // the assumption is that the write will be committed by the aforementioned higher-level transaction.
            if (realm.IsInTransaction)
                return function(realm);

            using var transaction = realm.BeginWrite();
            var result = function(realm);
            transaction.Commit();
            return result;
        }
    }
}
