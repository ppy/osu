// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using MessagePack;

#nullable enable

namespace osu.Game.Online.Multiplayer.MatchRulesets.TeamVs
{
    [MessagePackObject]
    public class TeamVsMatchRoomState : MatchRulesetRoomState
    {
        [Key(0)]
        public List<MultiplayerTeam> Teams { get; set; } = new List<MultiplayerTeam>();

        public static TeamVsMatchRoomState CreateDefault() =>
            new TeamVsMatchRoomState
            {
                Teams =
                {
                    new MultiplayerTeam { ID = 0, Name = "Team Red" },
                    new MultiplayerTeam { ID = 1, Name = "Team Blue" },
                }
            };
    }
}
