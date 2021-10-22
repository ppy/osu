// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// A request from a user to perform an action specific to the current match type.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    [Union(0, typeof(ChangeTeamRequest))] // IMPORTANT: Add rules to SignalRUnionWorkaroundResolver for new derived types.
    public abstract class MatchUserRequest
    {
    }
}
