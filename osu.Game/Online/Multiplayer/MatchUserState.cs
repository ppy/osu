// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;

#nullable enable

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// User specific state for the current match type.
    /// Can be used to contain any state which should be used before or during match gameplay.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    [Union(0, typeof(TeamVersusUserState))] // IMPORTANT: Add rules to SignalRUnionWorkaroundResolver for new derived types.
    public abstract class MatchUserState
    {
    }
}
