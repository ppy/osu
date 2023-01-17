// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;

namespace osu.Game.Online.Multiplayer.MatchTypes.TeamVersus
{
    [MessagePackObject]
    public class ChangeTeamRequest : MatchUserRequest
    {
        [Key(0)]
        public int TeamID { get; set; }
    }
}
