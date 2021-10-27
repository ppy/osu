// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

namespace osu.Game.Database
{
    public interface IHasOnlineID
    {
        /// <summary>
        /// The server-side ID representing this instance, if one exists. Any value 0 or less denotes a missing ID.
        /// </summary>
        /// <remarks>
        /// Generally we use -1 when specifying "missing" in code, but values of 0 are also considered missing as the online source
        /// is generally a MySQL autoincrement value, which can never be 0.
        /// </remarks>
        int OnlineID { get; }
    }
}
