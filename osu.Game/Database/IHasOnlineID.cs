// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

#nullable enable

namespace osu.Game.Database
{
    public interface IHasOnlineID<out T>
        where T : IEquatable<T>
    {
        /// <summary>
        /// The server-side ID representing this instance, if one exists. Any value 0 or less denotes a missing ID (except in special cases where autoincrement is not used, like rulesets).
        /// </summary>
        /// <remarks>
        /// Generally we use -1 when specifying "missing" in code, but values of 0 are also considered missing as the online source
        /// is generally a MySQL autoincrement value, which can never be 0.
        /// </remarks>
        T OnlineID { get; }
    }
}
