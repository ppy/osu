// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using MessagePack;

namespace osu.Game.Online.Multiplayer.MatchRulesets.TeamVs
{
    public class TeamVsMatchRoomState : MatchRulesetRoomState
    {
        [Key(0)]
        public List<MultiplayerTeam> Teams { get; set; }
    }
}
