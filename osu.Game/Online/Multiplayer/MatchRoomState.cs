// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;

#nullable enable

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// Room-wide state for the current match type.
    /// Can be used to contain any state which should be used before or during match gameplay.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    [Union(0, typeof(TeamVersusRoomState))]
    // TODO: this will need to be abstract or interface when/if we get messagepack working. for now it isn't as it breaks json serialisation.
    public class MatchRoomState
    {
    }
}
