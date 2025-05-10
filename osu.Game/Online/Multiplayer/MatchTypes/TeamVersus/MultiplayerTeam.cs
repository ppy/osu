// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Multiplayer.MatchTypes.TeamVersus
{
    [Serializable]
    [MessagePackObject]
    public class MultiplayerTeam
    {
        [Key(0)]
        public int ID { get; set; }

        [Key(1)]
        public string Name { get; set; } = string.Empty;
    }
}
