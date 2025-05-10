// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using MessagePack;

namespace osu.Game.Online.Multiplayer.MatchTypes.TeamVersus
{
    [MessagePackObject]
    public class TeamVersusRoomState : MatchRoomState
    {
        [Key(0)]
        public List<MultiplayerTeam> Teams { get; set; } = new List<MultiplayerTeam>();

        public static TeamVersusRoomState CreateDefault() =>
            new TeamVersusRoomState
            {
                Teams =
                {
                    new MultiplayerTeam { ID = 0, Name = "Team Red" },
                    new MultiplayerTeam { ID = 1, Name = "Team Blue" },
                }
            };
    }
}
