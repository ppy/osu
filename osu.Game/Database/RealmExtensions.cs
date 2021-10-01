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
    }
}
