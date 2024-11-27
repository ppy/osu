// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Online.Multiplayer
{
    public enum QueueMode
    {
        // used for osu-web deserialization so names shouldn't be changed.

        [Description("Host only")]
        HostOnly,

        [Description("All players")]
        AllPlayers,

        [Description("All players (round robin)")]
        AllPlayersRoundRobin
    }
}
